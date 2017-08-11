# Fusonic-Git-Backup

## What is Fusonic-Git-Backup?
Fusonic-Git-Backup is a dotnet core based tool to backup your github, gitlab and bitbucket 
repositories in one run.

## Features
* Full Docker support
* Email notification in error case
* Configure the tool individually in the app-settings.json
* Backup your repos with "git clone --mirror" to ensure the full backup of the repos with all branches
* Rebuild a fully functional repository from your backup with a simple "git push"
* Automaticaly builds a clear backup folder structure for you. (date/org/repopath/files)

## Setup
### Config File
If you want to run the tool inside the docker container, set the "Destination" to "/app/GitBackup". 
In the projects root folder you can find the app-settings.json to setup the tool for your needs. 
Enter the following user specific information and replace the placeholders to execute the tool properly:

    {
      "Git": [
        {
          "Type": "Bitbucket",
          "Username": "Bituser123",
          "PersonalAccessToken": "YourPrivateAccessToken"
        },
        {
          "Type": "Github",
          "Username": "Bituser123",
          "PersonalAccessToken": "YourPrivateAccessToken"
        },
        {
          "Type": "Gitlab",
          "Username": "YourUser1",
          "PersonalAccessToken": "YourPrivateAccessToken"
        },
        {
          "Type": "Gitlab",
          "Username": "YourUser2",
          "PersonalAccessToken": "YourPrivateAccessToken"
        }
      ],
      "Backup": {
        "Local": {
          "DeleteAfterDays": "1",
          "Destination": "/my/absoulte/path/to/backup/directory"
        }
      },
      "Mail": {
        "Host": "mailhoster.net",
        "Port": "25",
        "Username": "your.email@mail.com",
        "Password": "yourpassword",
        "UseSsl": "false",
        "Sender": {
          "Address": "your.email@mail.com",
          "Name": "Max Musterman"
        },
        "Receiver": {
          "Address": "Receiver.email@mail.com",
          "Name": "Manuel Musterman"
        }
      },
      "DeadmanssnitchUrl": "You can delete this if you dont use a dead man snitch"
    }

* Git: In this section you can configure your Gitlab, Github and Bitbucket account. If you dont 
need one or two of the connections, simply remove them from the config. You can configure as many
users as you want.
    * Type: Enter the type of your Account here. It should be either "Bitbucket", "Github" or 
    "Gitlab".
    * Username: Enter the username of your account here. You can find your username in your user 
    settings on each platform.
    * PersonalAccessToken: On each platform the "PersonalAccessToken" is named differently. 
    You need the generate a "PersonalAccessToken" for each platform to establish a connection 
    with the Git-Api and to grant the right permissions to your backup account. Generate your 
    passwords on their websites. Read permissions for repos should be enough.
        * Bitbucket: Go to "user settings -> App password" to generate your password.
        * Gitlab: Go to "user settings -> Access Tokens" to generate a token. Watch out for 
        the expire date. Gitlab is the only platform that lets the token expire after a given 
        time. Renew your token when your token expires. You can select the date by your own.
        * Github: Go to "user settings -> Personal access tokens" to generate a token.
    * Backup: Here you can configure you backup settings.
        * Local: Configure to store your backups directly on your machine.
            * DeleteAfterDays: Tells after how many days an old backup should be deleted 
            automaticaly.
            * Destination: The path to the folder where the backups should be stored. If you use Docker, 
            this sould be set to /app/GitBackup
    * Mail: Setup the Mailserver to send messages in error case. Get all the informations about 
    your mail settings from your mail server provider. For example see the gmail settings: 
    https://support.google.com/a/answer/176600?hl=en
        * Host: The address, IP or Url, to you mail server host.
        * Port: The prefered port.
        * Username: On gmail for example, its your email address
        * Password: The password of your email account
        * UseSsl: Tells if you want to use a ssh connection (needed if you use port 465)
        * Sender: Information about the sender of the emails:
            * Address: Address of the sender
            * Name: Name of the sender
        * Receiver: Information about the Receiver of the emails:
            * Address: Address of the Receiver
            * Name: Name of the Receiver
* DeadmanssnitchUrl: Paste your uri to your deadmansnitch here to call it whenever the backup succeeds.

## Run GitBackup locally
To run your tool localy you need to follow three steps:

### Install dotnet and git
Install dotnet and git on your machine to run your tool.

### Execute the tool
Now after configuring ssh, the app-settings.json and a dotnet environment you are all done and 
can try out the tool by running the following command in the commandline in your tools directory

    > cd /path/to/tool
    > dotnet run
    
You can either use the run command to run it or create a release folder to directly execute your
compiled dll file. This will store your dll's in the out folder.

    > cd /path/to/tool
    > dotnet publish git-backup.sln -c Release -o out

### Local Cron-Job
To run your backup tool as often as you want, install a cronjob, that executes dotnet run or your
compiled dll file. With your dll file it will look something like this:

    @midnight dotnet /path/to/tool/out/fusonic-git-backup.dll
    
## Run GitBackup with Docker
!IMPORTAINT!: Make sure that your "Destination" path in the app-settings.json is still set to
/app/GitBackup since this will be the woking directory of the tool inside the docker container.

You can build the docker image on you own but the fastest way to run your tool with docker is to 
download the already built image from the Git repository and execute the downloaded docker image 
as follows:

    sudo docker run -v /ABSOLUTE/PATH/TO/BACKUP/DEST:/app/GitBackup -v /ABSOLUTE/PATH/TO/YOUR/CONSTUM/app-settings.overwrite.json:/app/app-settings.json 600138a77e6c

* /ABSOLUTEPATH/TO/BACKUP/DEST => Set your absolute path to the destination path on your real machine here. This path 
will be mounted to the /app/GitBackup witch is the working path of the tool inside the docker container.
* /ABSOLUTE/PATH/TO/YOUR/CONSTUM => You need to mount your app-settings.json to tell the file what it has
to do.

### Docker cron job
To execute the tool and create backups after regular time spans, implement a cron job with the docker run comand from above.

## Restore a Backup
To restore a backup in a new repo create a new git repo and 'git push' the backup folder to the new repo.

## Deadman Snitch
Configure a Deaman Snitch to ensure your backup program is running as expected. The Deadman Snitch will wait for a life signal in a configurable period of time.
The tool will send a request to satisfy the Snitch whenever it gets executed successfully. If the tool fails it can not send a request to satisfy the Snitch in the configured 
time period and the snitch will send you an email with an error alert. For example this can be useful if the full server shuts down and you wont get any other error messages, 
since the tool will not even execute.

## FAQ
### How often does the projects create backups?
Run the tool every day to get a daily backup, or as often as you want. For example with a cron-job.
The program will create a full backup, each time it gets executed.

### How do I know if a buckup failed?
If a backup fails, an email will be sent to the address configured in the app-settings.json file.
The email includes the full exception message of the error to serve you all the information you need
to debug the tool.

--------------- Happy backuping! ---------------