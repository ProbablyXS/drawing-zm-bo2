namespace AssaultCubeHack
{
    class Matrix
    {
        //Memory layout of data will affect order of matrix. 
        //DirectX: Usualy Row-Major
        //OpenGL: Usualy Column-Major

        ////OPENGL    X,   Y,   Z,   W
        public float m11, m12, m13, m14; //00, 01, 02, 03 'X       DIRECT X
        public float m21, m22, m23, m24; //04, 05, 06, 07 'Y
        public float m31, m32, m33, m34; //08, 09, 10, 11 'Z
        public float m41, m42, m43, m44; //12, 13, 14, 15 'W


        public bool WorldToScreen(Vector3 worldPos, int width, int height, out Vector2 screenPos)
        {

            //multiply vector against matrix
            //float screenX = (m11 * worldPos.x) + (m21 * worldPos.y) + (m31 * worldPos.z) + m41; //OPENGL
            //float screenY = (m12 * worldPos.x) + (m22 * worldPos.y) + (m32 * worldPos.z) + m42;
            //float screenW = (m14 * worldPos.x) + (m24 * worldPos.y) + (m34 * worldPos.z) + m44;

            float screenX = (m11 * worldPos.x) + (m12 * worldPos.y) + (m13 * worldPos.z) + m14; //DIRECTX
            float screenY = (m21 * worldPos.x) + (m22 * worldPos.y) + (m23 * worldPos.z) + m24;
            float screenW = (m41 * worldPos.x) + (m42 * worldPos.y) + (m43 * worldPos.z) + m44;

            //camera position (eye level/middle of screen)
            float camX = width / 2f;
            float camY = height / 2f;


            //convert to homogeneous position
            float x = camX + (camX * screenX / screenW);
            float y = camY - (camY * screenY / screenW);

            screenPos = new Vector2(x, y);

            //check if object is behind camera / off screen (not visible)
            //w = z where z is relative to the camera 
            return (screenW > 0.001f);
        }
    }
}
