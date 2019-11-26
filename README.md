# AmisApp
A 3D, match-picture-with-name game whose pictures and names are customizable

Setup (Windows only)
=

- Download and extract the compressed build file
- Run the AmisApp executable, check the "Use local images" box
- Choose your difficulty and begin!

To change the pictures and names that the game displays do the following:
- Go into folder AmisApp_Data/AddedResources. NOTE: 9 images are needed, the first 8 correspond to ones that appear as selections. The 9th is simply a backdrop
- In this folder, you will find a names.txt text file, and various image files

The names in the .txt file are in a specific order: 
- The first four names correspond to images that are LANDSCAPE, the next 4 are PORTRAIT
- The 9th image is a LANDSCAPE image
- To modify the names and images, replace the current names with the needed names, and replace the images with the needed images. Make sure that the image files’ names are identical to the typed names in the names.txt file. Also make sure that the first four names are landscape and next four are portrait
- Use ONLY .jpg or .jpeg file extensions for the images. The optimal ratio of length:width for each image is 2:3

History and Notes
=

This app evolved from a prototype of a game I created for my friends. The original game had 8 hardcoded images, each of which was a picture me and one of my friends from school. These images were the inspiration behind this improved version's name: Amis. Amis app includes several improvements over the first game: 
- UI options to quit, go to login screen, and view the animation of image without needing to complete the game
- The ability to pull images from a website (for a time)
- Android build (deprecated, worked based on the website)

I made the mistake of hardcoding the url of a free hosting website for the "online" component of the game. The "Username" entered here corresponded to a directory I created on the server. This has the same file structure as the local "added resources" folder. I need to update the url in the game at some point to look at my new linux server.

Pic orbit is the next step in the evolution of this series of image display application. It emphasizes the showcase elements of the games that preceded, removing the "game" element. It supports the display of any number of images, of any ratio and supports both png and jpg file formats concurrently.
