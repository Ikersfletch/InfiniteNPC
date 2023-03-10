# InfiniteNPC

A Work-in-Progress Terria mod which adds customizable NPCs to Terraria.


Completed Feature List:
 - Interacting with them opens up an equipment menu where you can change every piece of an NPC.
 - Petrified Souls and Souls of Respite to implement their spawning into the game.
 - A soft API for Developers to add custom AI when unique weapons are used.
 - NPCs render map icons.
 
Partial Completion:
 - NPCs can use any weapon well enough.
    ( Summon weapons are broken and might not *be* fixable. )
 - The NPCs inventory can be locked to prevent further customization. Useful for map makers.
    ( This functionality is in the code, not in the game.)
 - The NPCs can be assigned housing like any other NPC.
    ( The game assigns housing based on NPC Type. All the generic NPCs are the same type. Ideally, each one would be given their own assignment.)
    ( They can still move into houses and be kicked out of houses, but cannot be individually assigned homes.)
 - Multiplayer Support
    ( This mod relies on internally overriding the client player. Multiplayer is here because it's untested, with a near-guarantee that it will not work.)

Unimplemented Goals:
 - NPCs can be marked as softcore
    -> Causes their deaths to act similar to vanilla NPCs, where they move in again.\
 - NPCs can have requests for the player:
    -> Be it Payment, Housing, or a fetch quest (like the Angler in vanilla) 
    -> Customizable for map makers.
 - NPCs can be assigned a task.
 - NPCs have dialog.
    -> The most realistic imlementation would be similar to the personality types found in Animal Crossing.
    -> Customizable for map makers.
 - NPCs have Happiness, which affects all of the above.

It's currently under an MIT license because I don't want to stop people from using it, but still want credit for what I worked on.
Feel free to clone or fork the repository- just leave the license and a link back here for the curious to find.
