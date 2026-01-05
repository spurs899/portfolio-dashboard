# Setup Instructions

## Configuration

Before running tests, you need to create your own `appsettings.json` file with your credentials.

1. Copy `appsettings.template.json` to `appsettings.json`:
   ```powershell
   Copy-Item appsettings.template.json appsettings.json
   ```

2. Edit `appsettings.json` and add your credentials:
   ```json
   {
     "Sharesies": {
       "Email": "your-actual-email@example.com",
       "Password": "your-actual-password"
     },
     "IBKR": {
       "Username": "your-actual-username",
       "Password": "your-actual-password"
     }
   }
   ```

**Note**: The `appsettings.json` file is git-ignored to prevent accidentally committing credentials.
