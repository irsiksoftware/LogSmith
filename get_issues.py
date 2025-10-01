#!/usr/bin/env python3
import subprocess
import json
import sys

try:
    result = subprocess.run(
        ['gh', 'issue', 'list', '--state', 'open', '--json', 'number,title,labels', '--limit', '50'],
        capture_output=True,
        text=True,
        check=True
    )
    issues = json.loads(result.stdout)

    # Filter for in-progress or ready
    for issue in issues:
        labels = [l['name'] for l in issue['labels']]
        if 'in-progress' in labels or 'ready' in labels:
            print(f"{issue['number']}: {issue['title']} - Labels: {','.join(labels)}")

except subprocess.CalledProcessError as e:
    print(f"Error: {e.stderr}", file=sys.stderr)
    sys.exit(1)
