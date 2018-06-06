This is a simple and fully self-contained example of a C# StarCraft 2 bot. The bot plays Terran and showcases various functions, such as training workers and marines, building depots and barracks, as well as attacking when a certain army size has been reached.

It has the laddermanager connection integrated and thus allows you to participate in the [sc2ai.net](sc2ai.net) ladder without requiring any modifications.

Setup:
- Make sure you have Starcraft 2 installed. It's free nowadays! Get it [here](https://us.battle.net/account/download/). You need to start the game successfully at least ONCE, otherwise nothing will work as expected.
- Make sure to have a good IDE installed, e.g. [Visual Studio](https://www.visualstudio.com/thank-you-downloading-visual-studio/?sku=Community&rel=15) (the community edition is free for private purposes), or Rider (costs about 100 USD per year)
- Make sure you have the [current map pack](https://github.com/Blizzard/s2client-proto#downloads) downloaded and unpacked into your Starcraft II/Maps folder. Don't worry about all the different packs, just go with the latest one. 
- Compile the solution and execute the resulting Bot.exe and it should start a game vs a computer opponent

Troubleshooting:
- Make sure you have the latest version of SC2 installed and launched it at least ONCE successfully! Otherwise it won't work.
- You might need to install or upgrade your dotnet framework. Typically this framework comes with the latest Windows updates, so if your Windows is up to date, you shouldn't require it. Still, if you encounter issues, make sure you have the [latest dotnet framework](https://www.microsoft.com/net/download/dotnet-framework-runtime) from Microsoft installed. The frameworks are backward compatible, so you should only really need the latest.
- By default it tries to launch the map Abiogenesis. You might not have this map. In that case just change it to a map you have in your maps/ folder within Program.cs
- It's a good idea to enable logging to files, by uncommenting the relevant lines in Logger.cs. This way you can always go back to the log files after a crash and figure out what went wrong. 
- Restart your PC
- If you still have problems, please contact us on the [#C-Sharp channel on our discord](http://discord.gg/qTZ65sh)


Notes:
- The chat stays on screen when playing a Computer opponent. In the ladder manager it works properly, so don't worry about it.
- The [wiki page](http://wiki.sc2ai.net/) has a lot of useful info that might answer a lot of your questions: 


Structure:

* Bot/Protocol: 
  * Contains all the SC2 protobuf classes. These manage the communication aspects between SC2 and other programs, in this case the C# bot. You don't need to worry about this really. Don't edit these files.
* Bot/Wrapper: 
  * Contains the wrapper that starts a 1v1 game vs the computer, or vs another bot if using the laddermanager. This is based on this [wrapper written by Simon Prins](https://raw.githubusercontent.com/SimonPrins/ExampleBot). Again, you shouldn't need to edit anything there, so don't worry about it.
* Bot/Abilities.cs:
  * These contain the ability identifiers required for virtually every action in the game. You can see its relatively short right now. If you ever need to find an ID search for *stableid.json* - a file that was created when you installed SC2. On Windows it is usually located at /Documents/StarCraft II/stableid.json. You will find all identifiers here.
* Bot/Units.cs:
  * These contain the unit identifiers required for every unit in the game. Again, this is taken directly from the stableid.json file. If anything changes in the future - this is what you'd peruse.
* Bot/RaxBot.cs:
  * This is the actual logic for the bot. In my case I called it RaxBot, because it just builds barracks and marines perpetually. You can rename this of course and improve it. 
* Bot/Program.cs:
  * This is the main entry point for the bot. In this file you can set the startup parameters for the program. Apart from these parameters you shouldn't need to change anything here. This part also manages the ladder manager connection.
* Bot/Controller.cs:
  * This is where the backend logic happens. You don't necessarily need this file - it just simplifies certain commands that I kept using a lot, such as Attack() or GetUnits(). Have a look at it. You will start to understand the logic required for the SC2API.







