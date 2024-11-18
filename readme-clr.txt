To create the AppGoodMusicWebApi

1. With Terminal in folder .scripts 
   macOs run: ./database-rebuild-all.sh
   Windows run: .\database-rebuild-all.ps1
   Ensure no errors from build, migration or database update

2. From Azure Data Studio you can now connect to the database
   Use connection string from user secrets:
   connection string corresponding to Tag
   "goodmusicefc.localhost.docker"

3. Use Azure Data Studio to execute SQL script DbContext/SqlScripts/initDatabase.sql

4. Run AppGoodMusicWebApi with or without debugger
   Without debugger: Open a Terminal in folder AppGoodMusicWebApi run: 
   dotnet run -lp https 

   open url: https://localhost:7066/swagger
   
5. Use endpoint Admin/Seed to seed the database, Admin/RemoveSeed to remove the seed
   Verify database seed with endpoint Guest/Info, 
   All endpoints on Controllers MusicGroups, Albums, Artists executable
