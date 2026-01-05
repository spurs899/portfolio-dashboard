# Remove Sensitive Data from Git History - Alternative Methods

## ✅ What's Been Done

1. ✅ `appsettings.json` removed from current commit
2. ✅ `appsettings.template.json` created as a template
3. ✅ `.gitignore` updated to ignore `appsettings.json`
4. ✅ Setup instructions added in README.md

## 🔒 Removing from Git History (Optional)

The file has been removed from the current commit, but it still exists in git history.
If you want to completely remove it from history, choose one of these methods:

### Method 1: Using BFG Repo-Cleaner (Recommended - Faster & Safer)

1. **Download BFG**:
   - Windows: Download from https://rtyley.github.io/bfg-repo-cleaner/
   - Or use: `choco install bfg-repo-cleaner`

2. **Clone a fresh bare copy**:
   ```powershell
   git clone --mirror git@github.com:your-repo.git repo-backup.git
   ```

3. **Run BFG**:
   ```powershell
   java -jar bfg.jar --delete-files appsettings.json repo-backup.git
   cd repo-backup.git
   git reflog expire --expire=now --all
   git gc --prune=now --aggressive
   ```

4. **Force push**:
   ```powershell
   git push --force
   ```

### Method 2: Using git filter-branch (Built-in)

**Run the provided script**:
```powershell
.\remove-appsettings-history.ps1
```

This script will:
- Rewrite all commits to remove the file
- Show you the steps to clean up
- Warn you about force-pushing

### Method 3: Do Nothing (Safe Option)

**If you're okay with the file being in history**:
- The file is now ignored going forward
- It won't appear in future commits
- The template file is tracked instead

This is fine if:
- The passwords were already changed
- The repository is private
- You haven't pushed to a public location

## 📋 After Removing from History

1. **Clean up**:
   ```powershell
   git gc --aggressive --prune=now
   ```

2. **Force push** (if you have a remote):
   ```powershell
   git push --force --all
   git push --force --tags
   ```

3. **Team members must**:
   ```powershell
   cd PortfolioDashboard
   git fetch origin
   git reset --hard origin/master
   ```

## 🛡️ Best Practices Going Forward

1. ✅ Never commit real credentials
2. ✅ Use template files for configuration
3. ✅ Keep sensitive files in `.gitignore`
4. ✅ Use environment variables or secret managers in production
5. ✅ Rotate any exposed credentials immediately

## 📝 Setting Up appsettings.json

Each developer should:
```powershell
cd PortfolioManager.Api.Tests
Copy-Item appsettings.template.json appsettings.json
# Edit appsettings.json with your credentials
```
