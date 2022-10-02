# BulkEditGoogleDrive

Creating multiple files at once in Google Drive is awful, because you have to do it manually.
With this simple and small program, you can create files in your Google Drive storage faster,
no matter what the type of the file is. It takes care of already created folders.

## Example

## Usage

### Requirements

The program connects to the Google Cloud, so you need to do some things before running the program.
Instead of creating a global Google Cloud application, you create your own application (adding the
needed API and creating the OAuth credentials), so you're the owner and maintainer of it. This is
so I don't have to register a new application, as this project is very small. Also, you have full
control over the requests, and if you know C# you can change whatever you want.

Right next you can see the list of steps you need to do before running the program. Every element
redirects you to the Google pages that explain everything clearly, including extra information that
you may want to know. I recommend you read related documents from the Google documentation to know
what you're doing.

1. [Create a Google Cloud project](https://developers.google.com/workspace/guides/create-project).
2. [Enable Google Workspace APIs](https://developers.google.com/workspace/guides/enable-apis).
    - In step 3, search for "Google Drive API".
3. [Configure the OAuth consent screen](https://developers.google.com/workspace/guides/configure-oauth-consent#configure_oauth_consent_register_your_app).
    - In step 5, add the scopes ".../auth/docs", ".../auth/drive", ".../auth/drive.metadata",
    ".../auth/drive.metadata.readonly", and ".../auth/drive.readonly".
4. [Create your OAuth client ID credentials](https://developers.google.com/workspace/guides/create-credentials#oauth-client-id)
    - In step 4, select "Desktop app" instead of "Web application".
    - Just after step 5, click on "Download JSON". Change the name of the downloaded file to
    `credentials.json`.
    - Skip step 6.

### Execution

After you've completed all of the steps, you are ready to run the program. You now have a Google
test application, with the Google Drive API enabled (so the program can manage your Google Drive
files), and with created OAuth credentials (that the program uses to access to your account).

To run the program, unzip the .rar file. You'll see a folder that has the executable file of the
program, alongside other files. Remember the .json file you downloaded earlier? Well,
you need to paste that file into the program's folder, at the top root (where the executable file
lies). **Don't forget to rename the credentials file to `credentials.json`.**