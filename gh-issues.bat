@echo off
gh issue list --state open --json number,title,labels --limit 50
