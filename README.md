# Match 3 demo
[Play it now!](http://rafael.se/games/match-3-demo/)

## Thoughts and decisions

### Vertical preparation
Keeping in mind the prevalent functionality of match 3 games, I have written the logic to easily expand to vertical matching, as well as refilling the board with new tiles from the top. Both the initial tile generation (BoardManager.GenerateTiles) and (BoardManager.FindMatches) are designed to have vertical matching implemented analogously, in the same single grid traversals, by adding corresponding accumulation counters and lists for all columns.

### Getting moving
Personally being immensely attracted to the use of physics in any possible way, I circumvented the physics engine prohibition by making a simple 2D particle simulation system of my own, used to create transitions and have the tiles move and wobble a bit. I have found that a little mechanics does wonders to the feeling in motions. It would be nice to have the tiles bounce on each other, flexing in shape a little, instead of intersecting like now. And the transition time delays could be used more elaborately to achieve a more natural effect. I just wanted to make the point of have tiles moving a bit separately from each other.

### Time flies
I found this an extraordinarily amusing project. I’ve been completely immersed, and would love to keep going. I focused on the feel and experience foremost, while trying to maintain cleanliness and consideration for the code reader. Given more time, I would refactor quite a lot though, dividing the somewhat bloated board manager into smaller and more manageable pieces. I would put more effort into naming and commenting things. I would adapt it for touch devices. I would do a lot more. I would make it perfect.
