# BulkCreateGoogleDrive

![GitHub all releases](https://img.shields.io/github/downloads/JoseDeFreitas/BulkCreateGoogleDrive/total)
![Supported OS versions](https://img.shields.io/badge/for-Windows%2C%20MacOS%2C%20Linux-blue)

Creating multiple files at once in Google Drive is awful, because you have to do it manually.
With this simple and small program, you can create files in your Google Drive storage faster,
no matter what the type of the file is. It takes care of already created folders.

## Example

https://user-images.githubusercontent.com/37962411/198579898-c6408801-7eb2-492a-8259-7e5a42899098.mp4

- Before the window that says "Google hasn't verified this app", you may see a prompt to choose
the Google account you want to use.
- The name of the app, which in the example video is "Test", may vary from the name of the app you
created in Google Cloud. You can choose any name you want.

## Usage

### Requirements

The program connects to Google Cloud, so you need to do some things before running the program.
Instead of creating a global Google Cloud application, you create your own application (adding the
needed API and creating the OAuth credentials), so you're the owner and maintainer of it. This is
so I don't have to register a new application, as this project is very small. Also, you have full
control over the requests, and if you know C# you can change whatever you want.

Below you can see the list of steps you need to do before running the program. Every element
redirects you to the Google pages that explain everything clearly, including extra information that
you may want to know. I recommend you read related documents from the Google documentation to know
what you're doing.

1. [Create a Google Cloud project](https://developers.google.com/workspace/guides/create-project).
2. [Enable Google Workspace APIs](https://developers.google.com/workspace/guides/enable-apis).
    - In step 3, search for "Google Drive API".
3. [Configure the OAuth consent screen](https://developers.google.com/workspace/guides/configure-oauth-consent#configure_oauth_consent_register_your_app).
    - In step 5, add the scope "**.../auth/drive.file**".
4. [Create your OAuth client ID credentials](https://developers.google.com/workspace/guides/create-credentials#oauth-client-id)
    - In step 4, select "Desktop app" instead of "Web application".
    - Just after step 5, click on "Download JSON". Change the name of the downloaded file to
    `credentials.json`.
    - Skip step 6.

### Execution

After you've completed all of the steps, you are ready to run the program. You now have a Google
test application, with the Google Drive API enabled (so the program can manage your Google Drive
files), and with created OAuth credentials (that the program uses to access your account).

To run the program, unzip the .rar file. You'll see a folder that has the executable file of the
program, alongside other files. Remember the .json file you downloaded earlier? Well,
you need to paste that file into the program's folder, at the top root (where the executable file
lies). **Don't forget to rename the credentials file to `credentials.json`.** When you run the
program the first time, a folder named `token.json` will be created.
**Keep that folder inside the main program folder**, it is required.

#### `files.txt` format

After that, fill the file `files.txt` with the correct format. The correct format is this:

```
FOLDER_NAME|n
FILE1.EXTENSION
FILE2.EXTENSION
```

Replace "`FOLDER_NAME`" with the name of the folder you want to create or select. The letter just
after the "|" symbol tells the program to number or not to number the files you want to create. If
you put "`n`", it won't number them; if you put "`y`", it will. This option adds numbers at the
start of the file names to keep all files organised. Refer to the example video to see how it
looks. Finally, replace `FILE1` with the name of the file you want and `.EXTENSION` with the type
of file you want. Add more files in the subsequent lines, like `FILE2.EXTENSION`.

#### Available file extensions

Although [Google Drive accepts many file types](https://support.google.com/drive/answer/37603), it
has its own types only available within Google. The program accepts only the four default types,
which are described in the following table:

|File type|Extension|
|---------|---------|
|Docs     |.gdo     |
|Sheets   |.gsh     |
|Slides   |.gsl     |
|Forms    |.gfo     |

If you want to add support for more file types, change the dictionary in the
[line 25](https://github.com/JoseDeFreitas/BulkEditGoogleDrive/blob/8aa76e7b79f06e02b41cb313e222f7f2275d9929/src/Program.cs#L25)
of the [Program.cs](src/Program.cs) file to add a file extension that corresponds to the name of
the type of the file. Refer to the [available MIME Types](https://developers.google.com/drive/api/guides/mime-types).

## Remarks

There are some things you should have in mind when running the program:

- When it finds a file with the same name and type, it won't be created. The same happens with
folders. You can indeed have folders and files from the same type with the same name, but I
implemented this exception to make sure files are not duplicated unnecessary when you want to
update a folder instead of creating a new one. This is the reason why this rule is propagated to
folders, as you wouldn't be able to update a folder that already exists.
- If you delete a folder but keep it inside the Google Drive bin, the program will detect that the
folder already exists. To bypass this behaviour, make sure to empty the bin. If you don't pay
attention to this, the program will create the files in the deleted folder, and you would be able
to see them when you restore it; however, the other files will be deleted.
