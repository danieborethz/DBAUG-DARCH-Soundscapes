# D-ARCH Unity Soundscapes Quickstart 

Welcome to the Unity Soundscapes Project form ETH Zürich! This Unity package enables auralizing 3D landscapes so that students of architecture and spatial planning disciplines and other users who are not familiar with environmental acoustics understand principles of soundscape and perception processes and can better imagine the soundscape. It uses Max 9 as an external sound engine.

## Installation & Setup 

1. Download the Unity package from the [releases page](https://github.com/danieborethz/DBAUG-DARCH-Soundscapes/releases)
2. Start up Max 9 and load the Patch
3. Create new unity project (universal 3D or 3D work with the provided assets) or open existing Unity Scene
4. Add the downloaded Unity package <img alt="Instructions on installing the unity package" src="/docs/images/Package_installation.jpg" />
5. Open up the Soundscape tool and load the audio library <img alt="Instructions on updating the audio library" src="/docs/images/Update_Audio_Library.jpg" />
6. In your scene, replace the camera with the Player prefab
7. Add the settings prefab to the scene as well
8. You're all set up! Check the [Components Guide section](#components-guide) for a detailed explanation of each component or open up the [Sample Scene](#sample-scene-walkthrough)
 
## Components Guide 
### Sound Source Audio Component
<img alt="Sound Source Audio Component" src="/docs/images/Sound_Source_Audio_Component.png" />

- **Category**: The category of the sound sources. Usually a folder in the provided audio library (See step 5 of Installation and setup)
- **Audio**: The audio files the Soundscape tool found in the respective category
- **Source Type**: Source channel on where the audio should play on the sound engine
- **Source**: Index of the source channel

<hr>

### Sound Source Generator
With the sound source generator you can generate two types: 
- Wind: Simulates rustling of leaves or needles of one or multiple trees
- Water: Simulates different water bodies such as rivers and fountains

### Sound Source Generator - Wind
<img alt="Sound Source Audio Component" src="/docs/images/Sound_Source_Generator_Wind.png" />
With wind you can either assign it to a single tree or a group objects of multiple trees (only the parent needs the component). If you assign it to a parent of a group then a forest size is calculated using the elements in the parent creating a wider sound instead of a single one. In the prefabs folder you find a forest prefab showcasing how it should be set up.

- **Foliage Type**: If the wind should either simulate a rustling of leaves or needles
- **Leaves/Tree Size**: Size of the leaves resp. tree
- **Channel**: On which stereo channel the generator should play. The generators always run on stereo

### Sound Source Generator - Water
<img alt="Sound Source Audio Component" src="/docs/images/Sound_Source_Generator_Water.png" />

- **Water Type**: If it either is a flow type water like river or a fountain type
- **Size**: Size of the water body
- **Channel**: On which stereo channel the generator should play. The generators always run on stereo
- **Splashing Time**: Only on water type splashing fountain. The duration is splashes water
- **Splashing Break**: Only on water type splashing fountain. The break between splashings.

<hr>

### Scene Manager

<hr>

## Sample Scene Walkthrough 


> [!NOTE]
> Coming soon

 
## Content Examples 

> [!NOTE]
> Coming soon


 

   
