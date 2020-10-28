using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Visual cue that lights up the borders when the omni gets close to its (euclidean) limits
// This is the directional version, that lights up certain borders when getting close to certain limits.

public class DirectionalOmniRangeCue : MonoBehaviour
{
    [SerializeField] HapticPlugin hapticDevice;
    private double[] maxRange;
    [SerializeField] float warningFraction = 0.9f;
    [SerializeField] Image left = null;
    [SerializeField] Image right = null;
    [SerializeField] Image up = null;
    [SerializeField] Image down = null;
    [SerializeField] Image all = null;
    Image[] borders = new Image[6];
    Vector3 maxSize;
    double[] warningRange = new double[6];

    // Start is called before the first frame updates
    void Start()
    {
        // Put border images in 1 array for easy useage
        borders[0] = left;
        borders[1] = down;
        borders[2] = all;
        borders[3] = right;
        borders[4] = up;
        borders[5] = all;

        // Don't know how to get the current joint angles so I have to do this the hard way...
        maxRange = hapticDevice.max_extents;
        maxRange[5] = 67; // This one is set too far (the omni gets in the way)

        // make transparent
        for (int i=0;i<6;i++)
            borders[i].color = new Color(borders[i].color.r, borders[i].color.g, borders[i].color.b, 0f);

        // Get full sizes to use for scaling of alpha value
        const int minX = 0;
        const int minY = 1;
        const int minZ = 2;
        const int maxX = 3;
        const int maxY = 4;
        const int maxZ = 5;

        maxSize = new Vector3(  
            (float)(maxRange[maxX] - maxRange[minX]),
            (float)(maxRange[maxY] - maxRange[minY]),
            (float)(maxRange[maxZ] - maxRange[minZ]));

        float r = warningFraction; // Fraction of the maxrange for which the warning will be given

        warningRange[0] = maxRange[0] + maxSize.x * (1 - r);
        warningRange[1] = maxRange[1] + maxSize.y * (1 - r);
        warningRange[2] = maxRange[2] + maxSize.z * (1 - r);
        warningRange[3] = maxRange[3] - maxSize.x * (1 - r);
        warningRange[4] = maxRange[4] - maxSize.y * (1 - r);
        warningRange[5] = maxRange[5] - maxSize.z * (1 - r);
    }

    // Update is called once per frame
    void Update()
    {
        if (hapticDevice.OmniSpring.anchorPosition3 == null) return;

        // make transparent
        for (int i = 0; i < 6; i++)
            borders[i].color = new Color(borders[i].color.r, borders[i].color.g, borders[i].color.b, 0f);

        float r = warningFraction; // Fraction of the maxrange for which the warning will be given

        double[] closeness = new double[6];

        closeness[0] = -(hapticDevice.stylusPositionRaw.x - warningRange[0]) / (maxSize.x * (1 - r));
        closeness[1] = -(hapticDevice.stylusPositionRaw.y - warningRange[1]) / (maxSize.y * (1 - r));
        closeness[2] = -(hapticDevice.stylusPositionRaw.z - warningRange[2]) / (maxSize.z * (1 - r));
        closeness[3] = (hapticDevice.stylusPositionRaw.x - warningRange[3]) / (maxSize.x * (1 - r));
        closeness[4] = (hapticDevice.stylusPositionRaw.y - warningRange[4]) / (maxSize.y * (1 - r));
        closeness[5] = (hapticDevice.stylusPositionRaw.z - warningRange[5]) / (maxSize.z * (1 - r));

        for (int i = 0; i < closeness.Length; i++)
        {
            if (closeness[i]>0)
                borders[i].color = new Color(borders[i].color.r, borders[i].color.g, borders[i].color.b, (float)closeness[i]);
        }
    }
}
