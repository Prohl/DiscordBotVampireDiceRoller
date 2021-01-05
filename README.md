# DiscordBotVampireDiceRoller

The VampireDiceRoller (Vadir) is a simple Discord Bot to help manage a Vampire V5 game over Discord. It includes a hungertracker for characters but this is not necessary if you only want to use it to roll dice.

## What to do

1. Install Visual Studio. 2017 and 2019 works, haven't tested it with VS Code.
2. Install the Discord.net plugin (https://github.com/discord-net/Discord.Net). Best with NuGet.
3. Downoad the sources and open the project in Visual Studio. When you compile it, there should be one error.
4. Follow the instructions under https://discord.foxbot.me/stable/guides/introduction/intro.html to create a Discord Bot. As soon as you got the token open the properties of the project, navigate to "Resources" and create an entry with the key "TOKEN" and the value is your token.
5. Now the solution should compile and directly connect to discord as your bot.

## How to use it

* The bot only responds, when he is adressed directly (@botname).
* When you type @botname help he will list all commands he understands with an explanation:
    * `roll`: let me roll some dice for you. You can either tell me to roll a number of dice and hungerdice with (roll X Y) or if you have an initialized character simply (roll X) and I will get the hungerdice from your characters hunger.
    * `reroll`: I will reroll up to three normal dice from your last roll. Without specification a reroll will reroll failures. You can specify what you want to reroll by adding 'c' for a critical, 's' for an success or 'f' for a failure (reroll cff).
    * `initchar`: followed by a name and your current hunger (initchar foo 2). Initializes your character.
    * `kill`: removes your current character and you can initialize a new one.
    * `rouse`: makes a rousecheck. When your character is initialized I tell you your new hunger, if it increases.
    * `rouse+`: makes a rousecheck with a reroll.
    * `hunger`: used to modify, set or get your hunger. To set it simply type hunger X and I will set your hunger to X. When you write +/- before X I will modify your hunger by X. Just hunger will reveal your current hunger.
    * `about`: shows the Version and some more info about vadir
* As a little gimmick there's a file called chatter.txt in the project. The file consists of key-value pairs divided by ":". When you copy that file into the executing folder of the bot, he will answer with a value if someone writes the key in a message (even if the bot is not adressed).
