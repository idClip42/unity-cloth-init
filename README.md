# unity-cloth-init
A script for initializing Unity cloth constraints based on vertex color and/or UV position

### Initial Modeling:
When creating the model you want to turn into Unity Cloth, specify which vertices will be free to move by either:
- Arranging their uvs in a line from left to right, with the leftmost uvs being the ones that will be held in place and the rightmost uvs being the ones that will move more freely. You will be able to determine later where along the x axis the cloth begins. This is useful for things like hair strands.
- Using vertex colors to color the more free vertices red, and the more held-in-place vertices black. It is recommended that you color in a gradient between black and red, rather than jumping between totally red and totally black. This is the recommended method of designing cloth.

### Using the Script:
Add your model to Unity and to the scene, and give it a cloth component and this script component.<br>
The script has three properties:
- **Max Move Distance**: How far (in meters) individual cloth vertices can travel from their original skinned mesh positions
- **Cloth Init Type**: Dropdown options for what the cloth weighting will be based off of:
  - **UV_Gradient**: Anything before the UV Threshold (discussed below) is static, and from there to the end it fade into cloth.
  - **Vert_Red**: Vertex colors determine level of cloth freedom, from black at 0 to red at the Max Move Distance.
  - **UV_Gradient_Vert_Red**: Both UV_Gradient and Vert_Red.
- **UV Threshold**: How far along the x axis model uvs can get before they start turning into cloth.

### Other Notes:
- This script currently only works on Skinned Mesh Renderers
- This script currently only works on GameObjects with a scale of 1
- A lot of the wording in this readme is awful
