/*

    Copyright (c) 2023 Pocketz World. All rights reserved.

*/

namespace Highrise.API.Models
{
    [Serializable]
    internal struct Vector3
    {
        public float x;
        public float y;
        public float z;

        public Vector3()
        {
            x = y = z = 0;
        }

        public Vector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
    }
}
