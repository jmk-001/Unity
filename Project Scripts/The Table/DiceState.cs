using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiceState : MonoBehaviour
{
    public Vector3Int DirectionValues;
    private Vector3Int OpposingDirectionValues;

    readonly List<int> FaceRepresent = new List<int>() {0, 1, 2, 3, 4, 5, 6};
    // Start is called before the first frame update
    void Start()
    {
        OpposingDirectionValues = 7 * Vector3Int.one - DirectionValues;
    }

    // Update is called once per frame
    void Update()
    {
    }

    public int reportFaceUp() {
        int faceUp = 0;
        if (  Vector3.Cross(Vector3.up, transform.right).magnitude < 0.5f)
            {
                if (Vector3.Dot(Vector3.up, transform.right) > 0)
                {
                    faceUp = FaceRepresent[DirectionValues.x];
                }
                else
                {
                    faceUp = FaceRepresent[OpposingDirectionValues.x];
                }
            }
        else if ( Vector3.Cross(Vector3.up, transform.up).magnitude <0.5f)
            {
                if (Vector3.Dot(Vector3.up, transform.up) > 0)
                {
                    faceUp = FaceRepresent[DirectionValues.y];
                }
                else
                {
                    faceUp = FaceRepresent[OpposingDirectionValues.y];
                }
            }
        else if ( Vector3.Cross(Vector3.up, transform.forward).magnitude <0.5f)
            {
                if (Vector3.Dot(Vector3.up, transform.forward) > 0)
                {
                    faceUp = FaceRepresent[DirectionValues.z];
                }
            else
                {
                    faceUp = FaceRepresent[OpposingDirectionValues.z];
                }
            }
        return faceUp;
    }
}
