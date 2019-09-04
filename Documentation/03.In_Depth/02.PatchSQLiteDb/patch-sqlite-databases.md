<!-- header
{
    "title": "Patch SqLite databases",
	"description": "How to patch existing databases and preserve data",
    "keywords": [ "database", "patch", "sqlite" ]
}
-->
# Patch SqLite databases #
## How patching of database(s) is working in CoreKraft? ##

### Kraft-Cli ###
In the Kraft-Cli there is a command which will execute the patching scripts:
```patchsqlite -tf D:\_Development\Ccf\Cleancode\Kraft\CoreKraft\Src\Authorization\Launcher\Data\ -td db@.sqlite```

where ```-tf``` is the target folder and ```-td``` is the target database. If you need to update multiple databases, please use the ```@``` sign at the end of the database name.

If the Kraft tool is not installed, please do the following:  
- Start cmd  
- ```dotnet tool install Kraft -g```  

If the Kraft tool is not up-to-date, please do the following:  
- Start cmd  
- ```dotnet tool update Kraft -g```  

### Folder structure ###
The patchsqlite command expects the following folder structure for the target folder:  
- Datafolder where the sqlite database(s) reside  
- Subfolder named ```Patches``` where the patch scripts live. These patch sqlite scripts follow this naming convention: ```DatabaseName_Version.extension  ```(e.g. Kraft_Users_1.0.1.sql).  
- The Cli tool supports semantic versioning.  

### Database(s) ###
Depending on what has been passed as argument the Kraft-Cli will update one or multiple databases.  
Steps taking place:  
- The Kraft-Cli tool will order the patch scripts a.) on the version and b.) filtered on the argument name.
- The Kraft-Cli will open the target database(s) and will look for a MetaInfo table with column Version. If there is no table a new one will be created.  
- One by one the Kraft-Cli tool will apply the patches. Every patch script will be executed in a separate transaction (e.g. please do not use transactions in your scripts).  

### Warnings… ###
```N.B.```: These warnings here are very specific for Sqlite and probably don’t apply for other databases.  

The problem:  
Add 2 additional columns of type INTEGER NOT NULL to an already populated table

Initial solution:  
The initial solution was to execute the following steps:  
- PRAGMA foreign_keys=off;  
- Rename the table to old  
- Create the new table  
- Copy everything from old to new  
- Delete the old table  
- PRAGMA foreign_keys=on;  


The problems surfaced after start using the patched database. The rename sqlite command changes the references of existing indexes and foreign keys to the renamed table. In our case above all indexes and foreign keys point to table old. When old is deleted in the script Sqlite doesn’t show any errors (e.g. show up at runtime). Additionally, it is very dangerous that during the patch process unintentional structural changes can mitigate performance.  

Final solution:  
- PRAGMA foreign_keys=off;  
- Create a new table (e.g. temp)  
- Copy everything from original to temp  
- Delete the original table  
- Create a new table with the new structure (e.g. original)  
- Copy everything from temp to original  
- Recreate the indexes (the cool part is that all Foreign Keys still reference the Original table)  
- Delete the temp table  
- PRAGMA foreign_keys=on;  


```sql  
PRAGMA foreign_keys=off;  

DROP TABLE IF EXISTS AspNetUsers_Temp;  

CREATE TABLE AspNetUsers_Temp (
    Id                   TEXT    NOT NULL
                                 CONSTRAINT PK_AspNetUsers PRIMARY KEY,
    UserName             TEXT,
    NormalizedUserName   TEXT,
    Email                TEXT,
    NormalizedEmail      TEXT,
    EmailConfirmed       INTEGER NOT NULL,
    PasswordHash         TEXT,
    SecurityStamp        TEXT,
    ConcurrencyStamp     TEXT,
    PhoneNumber          TEXT,
    PhoneNumberConfirmed INTEGER NOT NULL,
    TwoFactorEnabled     INTEGER NOT NULL,
    LockoutEnd           TEXT,
    LockoutEnabled       INTEGER NOT NULL,
    AccessFailedCount    INTEGER NOT NULL,
    FirstName            TEXT,
    LastName             TEXT,
    CommunicationConsent INTEGER NOT NULL,
    PrivacyConsent       INTEGER NOT NULL,
    ChallengeResponse    TEXT,
    AvatarImage          BLOB,
    AvatarMimeType       TEXT
);  

INSERT INTO AspNetUsers_Temp (Id, UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed, PasswordHash, SecurityStamp, ConcurrencyStamp, PhoneNumber, PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnd, LockoutEnabled, AccessFailedCount, FirstName, LastName, CommunicationConsent, PrivacyConsent, ChallengeResponse, AvatarImage, AvatarMimeType)
  SELECT Id, UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed, PasswordHash, SecurityStamp, ConcurrencyStamp, PhoneNumber, PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnd, LockoutEnabled, AccessFailedCount, FirstName, LastName, 1, 1, ChallengeResponse, AvatarImage, AvatarMimeType
  FROM AspNetUsers;  

DROP TABLE AspNetUsers;  
CREATE TABLE AspNetUsers (
    Id                   TEXT    NOT NULL
                                 CONSTRAINT PK_AspNetUsers PRIMARY KEY,
    UserName             TEXT,
    NormalizedUserName   TEXT,
    Email                TEXT,
    NormalizedEmail      TEXT,
    EmailConfirmed       INTEGER NOT NULL,
    PasswordHash         TEXT,
    SecurityStamp        TEXT,
    ConcurrencyStamp     TEXT,
    PhoneNumber          TEXT,
    PhoneNumberConfirmed INTEGER NOT NULL,
    TwoFactorEnabled     INTEGER NOT NULL,
    LockoutEnd           TEXT,
    LockoutEnabled       INTEGER NOT NULL,
    AccessFailedCount    INTEGER NOT NULL,
    FirstName            TEXT,
    LastName             TEXT,
    CommunicationConsent INTEGER NOT NULL,
    PrivacyConsent       INTEGER NOT NULL,
    ChallengeResponse    TEXT,
    AvatarImage          BLOB,
    AvatarMimeType       TEXT
);  

INSERT INTO AspNetUsers (Id, UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed, PasswordHash, SecurityStamp, ConcurrencyStamp, PhoneNumber, PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnd, LockoutEnabled, AccessFailedCount, FirstName, LastName, CommunicationConsent, PrivacyConsent, ChallengeResponse, AvatarImage, AvatarMimeType)
  SELECT Id, UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed, PasswordHash, SecurityStamp, ConcurrencyStamp, PhoneNumber, PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnd, LockoutEnabled, AccessFailedCount, FirstName, LastName, 1, 1, ChallengeResponse, AvatarImage, AvatarMimeType
  FROM AspNetUsers_Temp;  

DROP INDEX IF EXISTS 'EmailIndex';  
CREATE INDEX 'EmailIndex' ON 'AspNetUsers' (
	'NormalizedEmail'
);  
DROP INDEX IF EXISTS 'UserNameIndex';  
CREATE UNIQUE INDEX 'UserNameIndex' ON 'AspNetUsers' (
	'NormalizedUserName'
);  

DROP TABLE AspNetUsers_Temp;  

PRAGMA foreign_keys=on  
```

[Back to README](../../../README.md)