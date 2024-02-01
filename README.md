# vecr-project-bo2-zm

Working in offline and online






 
Security :

Using mongodb to getting all offset(s) from the db.
Each game(s) has specified table and offset(s).

Security 0) Obfuscation (Using ConfuserEx software)

Security 1) The launcher checking from the ftp if the version file is the same for starting
properly.

Security 2) Need to find the launcher key for each game (Force start program with the launcher)
The launcher need to have a encrypted key for starting the software for each game(s)

Security 3) After have selected a specified game the launcher going to be taking the link corresponding with your
game and downloading the software.
Each link address is encrypted as : base64 and AES

AES.Key = Jw9B69Tv6w61J2xCAPtZaqegflOXYeez
AES.IV = SU7VLJvqBnh1ohwV

Base64 do not have any protected key.

Request for generate your STRING DATA
REST API KEY = j6n0s0uf7EJAvvFcml0b

Need to know the key for deciphe each link address.

Security 4) Every 30 minutes the REST API activate one function for changing the access code.
The access code is requested when you start the launcher after selecting your game.

Security 5) Need to login with the same HWID.

HWID have : 
- email
- discordId
- uuid
- pcName
- machineId
- macAddress

Your license is locked to one computer and can be change with your permission.

Security 6) Each request for getting offset(s) is protected by on key 
You need to know the multiple encrypt key for opening the REST API data.

Security 7) When you start the software an function going to be start for checking every 30 secondes
to compare if your hwid is true.

Security 8) When you start the software an function going to be start for checking every 30 secondes
to view if you don't have any program named "hack", "cheat", "ollydbg", "hxd", "x32dbg", "ida64"
