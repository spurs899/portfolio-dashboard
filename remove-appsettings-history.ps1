# Script to Remove Sensitive Data from Git History
# WARNING: This rewrites git history. Only run if you haven't pushed to a shared remote,
# or if you're prepared to force-push and have all team members re-clone.

Write-Host "⚠️  WARNING: This will rewrite git history!" -ForegroundColor Yellow
Write-Host "Only proceed if:" -ForegroundColor Yellow
Write-Host "  1. You haven't pushed to a remote repository yet, OR" -ForegroundColor Yellow
Write-Host "  2. You're the only one using this repository, OR" -ForegroundColor Yellow
Write-Host "  3. You're prepared to force-push and have everyone re-clone" -ForegroundColor Yellow
Write-Host ""

$response = Read-Host "Do you want to continue? (yes/no)"
if ($response -ne "yes") {
    Write-Host "Aborted." -ForegroundColor Green
    exit
}

Write-Host ""
Write-Host "Removing PortfolioManager.Api.Tests/appsettings.json from all commits..." -ForegroundColor Cyan

# Use git filter-branch to remove the file from all commits
git filter-branch --force --index-filter `
  "git rm --cached --ignore-unmatch PortfolioManager.Api.Tests/appsettings.json" `
  --prune-empty --tag-name-filter cat -- --all

Write-Host ""
Write-Host "✅ File removed from history" -ForegroundColor Green
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "1. Run: git gc --aggressive --prune=now" -ForegroundColor White
Write-Host "   (This will clean up the old objects)" -ForegroundColor Gray
Write-Host ""
Write-Host "2. If you have a remote repository:" -ForegroundColor White
Write-Host "   Run: git push --force --all" -ForegroundColor White
Write-Host "   (This will force-push the rewritten history)" -ForegroundColor Gray
Write-Host ""
Write-Host "3. Notify team members to re-clone the repository" -ForegroundColor White
Write-Host ""
