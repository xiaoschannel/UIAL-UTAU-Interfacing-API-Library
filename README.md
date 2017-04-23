# UIAL: UTAU-Interfacing API Library

This is an open-source, cross-platform library written in C#, for those who wish to create vocal synth softwares, resamplers or plugins that works with UTAU or its related file format. MIT License.

Current features and completion:

 * UST: UST analysis and synthesis. Kinda works, still being unit-tested
 * VB: VB related files (oto, character.txt, prefixmap). Code done except character.txt, minimal testing done.
 * Mrq4Cs:  Moresampler compatibility. Code done, minimal testing done. (did i do it for version 2?)
 * Engine: Resampler/Wavtool interface. Only sketches done, cannot convert a note to parameters yet. Forward compatibility?
 * Developer Tools: (Windows only) For extracting and manipulating files I used for testing.

Please feel free to use the issue tracker and send pull requests.

Special Thanks to:

Everyone listed in "UST lists used for testing.xlsx" for sharing their USTs, Everyone in UTAUサークル（仮）(https://twitter.com/utaucircle) for helpful discussion and generous help.

Masanori Morise (http://ml.cs.yamanashi.ac.jp/research/index.html) 
	For WORLD.
    
Hua Kanru (https://github.com/Sleepwalking/) 
	For libllsm and a helpful discussion.
    
Zteer (http://z-server.game.coocan.jp/utau/utautop.html) 
	For tn_fnds and the infographics on UTAU's parameters.
    
Kbinani (https://github.com/kbinani)
	For Cadencii.
    
Acnak (https://github.com/acknak)
	For Nakloid.

Sugita Akira (https://github.com/stakira/)
	For OpenUtau.
    
ちていこ (https://twitter.com/chiteico)
	For many helpful advices.
    
Unfortunately, UIAL is built on my own utility library, zut. You will need to get that to compile my code. Feel free to suggest better alternatives!

UIAL uses UDE for character encoding detection.