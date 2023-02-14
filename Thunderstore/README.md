# Extradimensional Items
A mod that aims to add ideas from another games, while extending the playstyles of existing characters\roles. Currently mod has 5 new items and 3 new equipment.

<table>
  <tr>
    <th>Icon (more text so icons are bigger)</th>
    <th>Name</th>
    <th>Decription</th>
    <th>Type</th>
  </tr>
  <tr>
    <td><img src="https://raw.githubusercontent.com/viliger2/ExtradimensionaItems/master/Thunderstore/texSheenIcon.png" alt="Sheen"></td>
    <td>Sheen</td>
    <td>After using non-primary ability gain one stack of buff. Dealing damage with primary ability while having said buff will cause target take additional <b>250%</b> base damage (<b>+250%</b> per stack) and consume one stack of buff. Buff can be stacked up to <b>2</b> (<b>+2</b> per stack) times.</td>
    <td>Uncommon</td>
  </tr>
  <tr>
    <td><img src="https://raw.githubusercontent.com/viliger2/ExtradimensionaItems/master/Thunderstore/texRespawnFlagIcon.png" alt="Checkpoint"></td>
    <td>Checkpoint</td>
    <td>Place a checkpoint on the ground. Upon death respawn at checkpoint's location, destroying it in the process. Unused checkpoint can be picked back up. Only one checkpoint can exist per player at a time. Picking up different equipment results in checkpoint being disabled and transformed back into equipment. <b>If Fuel Cells are present, one will be consumed in place of checkpoint.</b><br>No cooldown.</td>
    <td>Equipment</td>
  </tr>
  <tr>
    <td><img src="https://raw.githubusercontent.com/viliger2/ExtradimensionaItems/master/Thunderstore/texSkullOfDoomIcon.png" alt="Skull of Impending Doom"></td>
    <td>Skull of Impending Doom</td>
    <td>Gain <b>100%</b> movement speed, while taking <b>10%</b> max health as damage every <b>3</b> seconds. Use again to disable the effect. <b>If Fuel Cells are present, gain additional <i>15%</i> movement speed and reduce damage by <i>15%</i> per stack.</b><br> No cooldown.</td>
    <td>Lunar Equipment</td>
  </tr>  
  <tr>
    <td><img src="https://raw.githubusercontent.com/viliger2/ExtradimensionaItems/master/Thunderstore/texRoyalGuardItemIconGood.png" alt="Pizza Box"></td>
    <td>Pizza Box</td>
    <td>Replace your Utility Skill with Block. Block allows you to enter a defensive stance for <b>0.5</b> (<b>+0.1</b> per stack) seconds, deflecting first non-periodic hit. Depending for how long you were in the stance gain 3 to 1 damage buff(s). If you have damage buffs when using Block, your Primary Skill gets replaced with Release. Using Release will consume all damage buffs and deal <b>1000%</b> (<b>+1000%</b> per stack) base damage per buff stack to everyone in <b>15m</b> radius. Release has increased proc coefficient. You can have up to <b>8</b> stacks of damage buff.</td>
    <td>Legendary</td>
  </tr>  
  <tr>
    <td><img src="https://raw.githubusercontent.com/viliger2/ExtradimensionaItems/master/Thunderstore/texCooldownReductionIcon.png" alt="Sorcerer's Pills"></td>
    <td>Sorcerer's Pills</td>
    <td>Gain <b>10%</b> (<b>+10%</b> per stack) cooldown reduction. Stacks hyperbolically, like Haste in WoW or Ability Haste in LoL, as in 100% will reduce cooldown by half, 200% by 3/4, etc. Corrupts all Soldier's Syringes.</td>
    <td>Void Common</td>
  </tr>  
  <tr>
    <td><img src="https://raw.githubusercontent.com/viliger2/ExtradimensionaItems/master/Thunderstore/texDamageOnCooldownIcon.png" alt="Fueling Bellows"></td>
    <td>Fueling Bellows</td>
    <td>For each ability and equipment on cooldown gain <b>10%</b> (<b>+5%</b> per stack) bonus damage.</td>
    <td>Uncommon</td>
  </tr>    
  <tr>
    <td><img src="https://raw.githubusercontent.com/viliger2/ExtradimensionaItems/master/Thunderstore/texChronoshiftIcon.png" alt="Chronoshift"></td>
    <td>Chronoshift</td>
    <td>Move <b>10</b> seconds back in time. Restores position, money, items, health, barrier, shield and skill cooldowns to the state they were. Snapshots are taken every <b>0.25</b> seconds.<br> <b>120</b> second cooldown.</td> 
    <td>Equipment</td>
  </tr>  
  <tr>
    <td><img src="https://raw.githubusercontent.com/viliger2/ExtradimensionaItems/master/Thunderstore/texAtmaIcon.png" alt="Atma's Impaler"></td>
    <td>Atma's Impaler</td>
    <td>Gain additional base damage equal to <b>0.5%</b> (<b>+0.5%</b> per stack) maximum health.</td>
    <td>Uncommon</td>
  </tr>   
</table>

Things marked in bold can be edited via config. Mod supports in-game config setup via RiskOfOptions and has full BetterUI support for every item.

If you find any bugs send me a message on discord at **viliger#9877** or submit an issue on github. I also need help with some of the assets (Atma needs a proper model, Pizza box needs good skill icons, etc), writing propper lore and models\animation extraction from game called Super Cloudbuilt. If you want to help, let me know.

## Known issues\things
<details>

  * Some effects and sounds might not work on third client (as in not server and not client that sound comes from), but as far as functionality everything should be working. 
  * Respawn Flag:
    * If MUL-T picks flag as first item, places it and then picks up item without using Retool flag convers back into the item, like if anothe equipment was picked up. It can be resolved by using Retool once with any equipment, at the start MUL-T has one equipment slot, until MUL-T uses Retool at least once with any equipment.
  * Chronoshift:
    * Oddly-shaped Opal doesn't reset.
    * Doesn't reset states, MUL-T won't swap back, Void Fiend won't get his energy\state, Railgunner will be scoped if she was scoped or still be overheated, etc.
    * Capitan does get his beacons back but he is hard capped to two beacons at the time in vanilla. Maybe with other mods it works I dunno.
    * Probably completely breaks custom characters that rely on buffs for states.
  * Sorcerer's Pills:
    * Cooldown reduction from Alien Head is applied after Sorcerer's Pills. For example, Commando's special has base cooldown of 9 seconds, 10 pills will bring it down to 4.5 and additional Alien Head will bring it down to 3.375. This is R2API behavior, not much I can do without not using it and even then, I think it is fine as it is.

</details>

## Credits
<details>

  * Models come from https://thebasemesh.com/ unless stated otherwise.
  * Pizza Box - model comes from https://sketchfab.com/3d-models/pizza-box-7c982c66dade4967961f13e1fea6c07a, sound effects come from DMC4 and DMC5, property of Capcom, effects come from World of Warcraft, property of Blizzard
  * Checkpoint - model, sounds and textures come from Super Cloudbuilt, property of Coilworks
  * Chronoshift - cut out from Ekko model from League of Legends, property of Riot Games
  * Skull of Doom - model from Tales of Monkey Island, property of Telltale Games, sounds come from World of Warcraft, property of Blizzard
</details>

## TODO\Future things
<details>

  * Implement item displays for modded characters.
  * Fix logbook item displays. Only Chronoshift currently has "display", as in it follows what I put in the code, but it needs a lot of fixing to look good.
  * Majority of items lack lore, you are free to submit a pull request or message me directly if you want to write it.
  * New model and icon for Atma, it looks terrible
  * Propper skill icons for Pizza Box
  * Code rewrite to Chronoshift at the very least, it is very janky at the moment
  * Quest system, where you pick quests in the lobby, for a price of course (0.6.0)
  * New Survivor (0.7.0)
  * New Stage (0.8.0)

</details>