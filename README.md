# ManageFilesFromGoogleSheet

Google Drive is one of the most used cloud services, for many reasons. Google Drive allows you
to create folders and add files in them. I've encountered the situation where I had so much
unorganised files that, when I wanted to change something in one file, I had to change the
subsequent ones to make them match the new change.

With this little program, you can create a Google Sheet file to use it as a template for how
do you want a specific folder to behave. Imagine the case you need to have many Google Docs files,
and instead of using an external app or addon you want to work only within Google Drive. Well, you
can just create a Google Sheet file and write all the files you want to create, alongside other
information, like the labels you want to put to them, if you want them to be starred or to show
you specific information, like how many words there are per Google Docs file.

In the end, you'll have a single Google Sheet file were all the information of the files of a
specific folder will be organised and ready to update the files and retrieve information from
the other folder. It may not sound very clear, so see the following example video to know
how it works.

## Example

## Usage

### Requirements

The program connects to the Google Cloud, so you need to do some things before running the program.
Instead of creating a global Google Cloud application, you create your own application (adding the
needed API and creating the OAuth credentials), so you're the owner and maintainer of it but just
run the program I created. This is so I don't have to register a new application, as this project
is very small. Also, you have full control over the requests, and if you know C# you can change
whatever you want.

Right next you can see the list of steps you need to do. Every element redirects you to the Google
pages that explain everything clearly, including extra information that you may want to know. I
recommend you read related documents from Google docs to know what you're doing.

1. [Create a Google Cloud project](https://developers.google.com/workspace/guides/create-project).
2. [Enable Google Workspace APIs](https://developers.google.com/workspace/guides/enable-apis).
    - In step 3, search for "Google Drive API".
3. [Configure the OAuth consent screen](https://developers.google.com/workspace/guides/configure-oauth-consent#configure_oauth_consent_register_your_app).
    - In step 5, add the scopes ".../auth/docs", ".../auth/drive", ".../auth/drive.metadata" and
    ".../auth/drive.metadata.readonly".
4. [Create your OAuth client ID credentials](https://developers.google.com/workspace/guides/create-credentials#oauth-client-id)
    - In step 4, select "Desktop app" instead of "Web application".
    - Just after step 5, click on "Download JSON".
    - Skip step 6.
5. Run the program.

### Execution
