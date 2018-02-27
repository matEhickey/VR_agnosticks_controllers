# VR_agnosticks_controllers  

## How to use  

Download all the repo, and import it in your project asset.  
Remove the default camera.  
Grab the "Simulator" prefab, and place it on your scene.  

You now can modify InputManagerController based on your defined actions/axis (by default, there are 2 actions by controller, dont call them gripPress, but by the action the user think it is (ex: grab/teleport/etc)  
Map for every controller needed the hardware input and the actions/axis.  

Get the axis by calling the method 'getAxis("left","Horizontal")' for getting the horizontal axis on the left controller.  
You can attach callback to boolean actions from the UI, like you do with every UI buttons.  

You can now attribute keyboard buttons to manipulate the simulation when there is no headset.  

Then run the tests and if everything is green you'r good to go.  
