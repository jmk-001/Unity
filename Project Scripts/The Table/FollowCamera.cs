using System.Collections;
using System.Collections.Generic;
using System.IO.Compression;
using FishNet.Object;
using UnityEngine;

public class FollowCamera : NetworkBehaviour
{
    public Transform cameraTransform;
    private Vector3 pos, fw, up;

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (!base.IsOwner)
        {
            enabled = false;
        }
        cameraTransform = Camera.main.transform;
        pos = -cameraTransform.transform.InverseTransformPoint(transform.position);
        fw = -cameraTransform.transform.InverseTransformDirection(transform.forward);
        up = cameraTransform.transform.InverseTransformDirection(transform.up);
    }

    void Update()
    {
        if (cameraTransform != null){
            var newpos = cameraTransform.transform.TransformPoint(pos);
            var newfw = cameraTransform.transform.TransformDirection(fw);
            var newup = cameraTransform.transform.TransformDirection(up);
            var newrot = Quaternion.LookRotation(newfw, newup);
            transform.position = newpos;
            transform.rotation = newrot;
        }
    }
}
