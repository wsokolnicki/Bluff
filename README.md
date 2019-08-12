# Bluff
--== There still might be some bugs that I haven't discovered yet. ==--

This is my first real game, done from scratch. 
It is a multiplayer card game for 2-8 players (no AI yet, maybe will be implemented in the future).

There are no rules or explanations inside the game. Few words about the game can be found here.

It is a poker based game, where deck contains of 24 cards, from 9 to Ace. 
Each player starts with only one card in hand and each card from all players' hands are taking part in the game. First player, who is chosen randomly, choose a poker value, that he thinks is in the game (among all of the cards in players' hands). For example High Card Ace or One Pair or even Royal Flush. Next player has to choose a poker value greater then previous one or can simply check the previous player. If he goes with the Check option, all cards are flipped and game checks if the chosen value was indeed in all cards in the game. If it was, than player who chcecked gets an extra card. If not, the player who chose that option gets an extra card. So in the next round, all of the players have still one card, except for the one player who has two.

Player who checked in previous round will begin the next round. If he lost this round, next player in line will become the first.

Player looses the game, when he has a specific number of cards in hand and gets another one. The number changes, depending on how many players play the game: 
- 2-4 players - 5 cards 
- 5-6 players - 4 cards 
- 7-8 players - 3 cards

That player is deactivated from all of the next rounds, but gets an ability to watch all of the players cards. Rest of the players still are playing according to the rules.

Game ends when there is only one player left on the table.

Game is called "Bluff" just to encourage players to bluff/lie during the game. For example, If you are a first player in given round and you have three cards: Nine, Ten and Ace you can say A pair of queens. Next player, seeing that you have three cards, will most likely think, that you realy got two queens, or at least one and he might say something greater than that. Another player may set everything he has on your lie (and say for example full house - queens over jacks). At the end it will come out that there is no queen among the cards at all. But if you want, you can also play safe and always go with the cards that you have in hand.

I hope this explains the principle of this game in a comprehensible way.
