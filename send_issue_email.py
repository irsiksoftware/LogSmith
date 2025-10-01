#!/usr/bin/env python3
"""
Send GitHub issue details with test results via Amazon SES.

Usage:
    python send_issue_email.py <issue_number>

Example:
    python send_issue_email.py 32
"""

import sys
import os
import re
import json
import subprocess
import configparser
import smtplib
from email.mime.multipart import MIMEMultipart
from email.mime.text import MIMEText
from email.mime.base import MIMEBase
from email import encoders
from datetime import datetime
from pathlib import Path


def load_settings(config_file='settings.ini'):
    """Load settings from INI file."""
    config = configparser.ConfigParser()
    config.read(config_file)
    return config


def get_github_issue(issue_number, repository):
    """Fetch GitHub issue details using gh CLI."""
    try:
        result = subprocess.run(
            ['gh', 'issue', 'view', str(issue_number), '--repo', repository, '--json',
             'number,title,body,url,state,labels,assignees,createdAt,updatedAt'],
            capture_output=True,
            text=True,
            check=True
        )
        return json.loads(result.stdout)
    except subprocess.CalledProcessError as e:
        print(f"Error fetching issue: {e.stderr}")
        sys.exit(1)
    except FileNotFoundError:
        print("Error: 'gh' command not found. Please install GitHub CLI.")
        sys.exit(1)


def find_latest_test_result(issue_number, output_dir='TestOutputs'):
    """Find the newest test result file for the given issue."""
    if not os.path.exists(output_dir):
        print(f"Warning: {output_dir} directory not found.")
        return None

    pattern = re.compile(rf'Test-Results-GH{issue_number}-(\d{{2}}-\d{{2}}-\d{{4}}-\d{{2}}-\d{{2}}-\d{{2}})\.html?$')
    matching_files = []

    for filename in os.listdir(output_dir):
        match = pattern.match(filename)
        if match:
            filepath = os.path.join(output_dir, filename)
            timestamp_str = match.group(1)
            # Parse timestamp: MM-DD-YYYY-HH-MM-SS
            try:
                timestamp = datetime.strptime(timestamp_str, '%m-%d-%Y-%H-%M-%S')
                matching_files.append((filepath, timestamp))
            except ValueError:
                continue

    if not matching_files:
        print(f"Warning: No test result files found for issue #{issue_number}")
        return None

    # Sort by timestamp and return the newest
    matching_files.sort(key=lambda x: x[1], reverse=True)
    return matching_files[0][0]


def format_issue_html(issue):
    """Format issue details as HTML."""
    labels = ', '.join([label['name'] for label in issue.get('labels', [])])
    assignees = ', '.join([assignee['login'] for assignee in issue.get('assignees', [])])

    html = f"""
    <html>
    <head>
        <style>
            body {{ font-family: Arial, sans-serif; margin: 20px; }}
            h2 {{ color: #333; }}
            .info {{ margin: 10px 0; }}
            .label {{ font-weight: bold; }}
            .body {{
                background-color: #f5f5f5;
                padding: 15px;
                border-left: 4px solid #0969da;
                margin: 20px 0;
            }}
            .link {{
                display: inline-block;
                margin: 20px 0;
                padding: 10px 20px;
                background-color: #0969da;
                color: white;
                text-decoration: none;
                border-radius: 5px;
            }}
        </style>
    </head>
    <body>
        <h2>Issue #{issue['number']}: {issue['title']}</h2>

        <div class="info">
            <span class="label">Status:</span> {issue['state']}
        </div>

        {f'<div class="info"><span class="label">Labels:</span> {labels}</div>' if labels else ''}

        {f'<div class="info"><span class="label">Assignees:</span> {assignees}</div>' if assignees else ''}

        <div class="info">
            <span class="label">Created:</span> {issue['createdAt']}
        </div>

        <div class="info">
            <span class="label">Updated:</span> {issue['updatedAt']}
        </div>

        <div class="body">
            <h3>Description:</h3>
            <pre>{issue.get('body', 'No description provided.')}</pre>
        </div>

        <a href="{issue['url']}" class="link">View Issue on GitHub</a>
    </body>
    </html>
    """
    return html


def send_email(config, issue, attachment_path=None, repo_name=''):
    """Send email via Amazon SES SMTP."""
    from_addr = config['Email']['FromAddress']
    to_addrs = [addr.strip() for addr in config['Email']['ToAddresses'].split(',')]
    smtp_host = config['Email']['SMTPHost']
    smtp_port = int(config['Email']['SMTPPort'])
    smtp_user = config['Email']['SMTPUsername']
    smtp_pass = config['Email']['SMTPPassword']

    # Create message
    msg = MIMEMultipart('alternative')
    subject_prefix = f"{repo_name} " if repo_name else ""
    msg['Subject'] = f"{subject_prefix}GH-{issue['number']}: {issue['title']}"
    msg['From'] = from_addr
    msg['To'] = ', '.join(to_addrs)

    # Create HTML body
    html_body = format_issue_html(issue)
    msg.attach(MIMEText(html_body, 'html'))

    # Attach test results if available
    if attachment_path and os.path.exists(attachment_path):
        filename = os.path.basename(attachment_path)
        with open(attachment_path, 'rb') as f:
            part = MIMEBase('application', 'octet-stream')
            part.set_payload(f.read())

        encoders.encode_base64(part)
        part.add_header('Content-Disposition', f'attachment; filename= {filename}')
        msg.attach(part)
        print(f"Attached: {filename}")

    # Send email
    try:
        with smtplib.SMTP(smtp_host, smtp_port) as server:
            server.starttls()
            server.login(smtp_user, smtp_pass)
            server.send_message(msg)

        print(f"Email sent successfully to: {', '.join(to_addrs)}")
        return True
    except Exception as e:
        print(f"Error sending email: {e}")
        return False


def main():
    if len(sys.argv) < 2:
        print("Usage: python send_issue_email.py <issue_number>")
        sys.exit(1)

    try:
        issue_number = int(sys.argv[1])
    except ValueError:
        print("Error: Issue number must be an integer")
        sys.exit(1)

    # Load configuration
    config = load_settings()
    repository = config['GitHub']['Repository']
    repo_name = config['GitHub'].get('RepositoryName', repository.split('/')[-1])
    output_dir = config['TestResults']['OutputDirectory']

    print(f"Fetching issue #{issue_number} from {repository}...")
    issue = get_github_issue(issue_number, repository)

    print(f"Found: {issue['title']}")

    # Find latest test result
    print(f"Searching for test results in {output_dir}...")
    attachment = find_latest_test_result(issue_number, output_dir)

    if attachment:
        print(f"Found test result: {os.path.basename(attachment)}")

    # Send email
    print("Sending email...")
    if send_email(config, issue, attachment, repo_name):
        print("Done!")
    else:
        sys.exit(1)


if __name__ == '__main__':
    main()
