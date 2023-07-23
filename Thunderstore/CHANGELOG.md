<details>
<summary>0.5.5 </summary>

* Fixed error on start up if BetterUI was not present, for real this time.
* Rewrote how text strings are filled. Now also dynamically updates with in-game config changes (this feature requires RiskOfOptions version 2.6.0 or higher, will probably throw errors if below).
	* _Thanks to Faby for telling me that my current implementation is shit, new implementation should lead to better performance overall._
* Added missing "restartRequired" flag to some options in RiskOfOptions, mainly those that are used in catalog initialization.
* Made all equipment Enigma incompatable.
	* _I've finally played with Enigma, for the first time in 200 hours, and as it turns out none of my equipment are Enigma-compatable. Skull instantly stops working, dealing damage once, Chronoshift does nothing (while potentially throwing errors in network play) and Flag instantly transforms back into equipment. This is all due to how I made them and how there are checks for what equipment player has. While I can fix it, I honestly don't want to, especially with Chronoshift, maybe on code rewrite._
* Adrenaline Core
	* Fixed pick up not rotating.
* Fueling Bellows
	* Added some additional logging, can be disabled in the config as usual.
* Skull of Impending Doom
	* Fixed damage, but not speed, buff appearing sometimes on clients when using different equipment.
</details>
<details>
<summary>0.5.4 </summary>

* Fixed error on start up if BetterUI was not present.
* Implemented item displays for Sniper and Rocket.
* Chronoshift
	* "Fixed" compatability issue with NetworkedTimedBuffs
		* _Technically the issue is fixed from NetworkedTimedBuffs side, but since I got the bug report might as well put it here._
* Sorcerer's Pills
	* Added information about what it corrupts to pickup text.
* Updated dependencies.
</details>
<details>
<summary>0.5.3 </summary>

* Adrenaline Core
	* Fixed errors and UI disappearing when using RiskUI
		* _Currently the UI portion of the item just disables itself when it can't find some things that RiskUI removes. I might add actual UI with appropriate style to support RiskUI, but for now, we just disable the leveling bar. It also opens another can of worms of supporting each UI that comes out (which I doubt more will happen but still) and I really don't wanna do that. In the meantime, you can check you current Adrenaline level by looking at the color of the item display's glow._
	* Fixed an issue when disabling UI in RiskOfOptions didn't actually disable it.
</details>
<details>
<summary>0.5.2 </summary>

* Adrenaline Core
	* New item.
* New console command "give_item_ai"
	* It can be used to give AI team items when either Artifact of Evolution is enabled or if players are in Void Fields. Won't work otherwise.
* Skull of Impending Doom
	* DamageType now also inculdes DoT.
* Atma's Impaler
	* BetterUI now shows actual bonus damage from the item instead of percent value.
</details>
<details>
<summary>0.5.1 </summary>

* Pizza Box
	* Tier replaced with Lunar
		* _After some thinking and suggestions, I decided to move Pizza Box into Lunar tier from Legendary. Simply put, the item is way too playstyle changing to be red, unless you deliberately want to play with it, finding it inside Stage 4 chest can lead to frustration, if you pick it up it can end your run and if you don't you just wasted gold on something that you can't even pick up. It doesn't suit Lunar tier stylistically, maybe something for later_.
	* Removed unneeded after parry grace buff, replaced it with in-game invincibility.
* Added item displays for Miner, Enforcer, Nemforcer and Paladin.
</details>
<details>
<summary>0.5.0 </summary>

* Initial release
</details>
