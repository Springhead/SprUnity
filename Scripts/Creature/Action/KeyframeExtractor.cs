using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubMovement1D{
    float x0;
    float t0;

    float x1;
    float t1;

    public SubMovement1D(float vp, float tp, float tw)
    {
        x0 = 0.0f;
        t0 = tp - tw;

        x1 = (8 * 2 * tw * vp) / 15;
        t1 = tp + tw;
    }

    public float GetCurrentPose(float t)
    {
        float delta = x1 - x0;

        if (t < t0) { return x0; }
        if (t1 < t) { return x1; }

        float s = (t - t0) / (t1 - t0);
        float r = 10 * Mathf.Pow(s, 3) - 15 * Mathf.Pow(s, 4) + 6 * Mathf.Pow(s, 5);

        return delta * r; // + x0いらない？
    }

    public float GetCurrentVelocity(float t)
    {
        float delta = (x1 - x0) / (t1 - t0);

        if (t < t0) { return 0; }
        if (t1 < t) { return 0; }

        float s = (t - t0) / (t1 - t0);
        float r = 30 * Mathf.Pow(s, 2) - 60 * Mathf.Pow(s, 3) + 30 * Mathf.Pow(s, 4);

        return delta * r;
    }
}
/*
public class Filter<Type> where Type: new()
{
    Type[] NPointsSmoothing(Type[] data)
    {
        Type[] filtered = new Type[data.Length];
        int width = 2;
        for(int i = 0; i < data.Length; i++)
        {
            Type c = data[i];
            for (int j = 0; j < width; j++) {
                c = c + data[Mathf.Max(0, i - j)];
                c = c + data[Mathf.Min(i + j, data.Length - 1)];
            }
        }
        return filtered;
    }
}
*/
public class KeyframeExtractor : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}


    // 速度データから特徴点の抽出
    // <!!> 何を返すべき？
    //      候補1:極大点と変曲点(ピークタイムのため) & 極小点(サブムーブの時間長決定のため) 
    //      候補2:極大点と変曲点 & 幅
    public List<SubMovement1D> ExtractFeatures(float[] velocityData, out List<int> maxi, out List<int> mini, out List<int> inf)
    {
        
        int dataLength = velocityData.Length;

        // フィルタによるノイズ除去
        int width = 2;
        float[] filtered = new float[velocityData.Length];
        for (int i = 0; i < velocityData.Length; i++)
        {
            float c = velocityData[i];
            for (int j = 0; j < width; j++)
            {
                c = c + velocityData[Mathf.Max(0, i - j)];
                c = c + velocityData[Mathf.Min(i + j, velocityData.Length - 1)];
            }
            filtered[i] = c / (2 * width + 1);
        }
        velocityData = filtered;

        // 極値点
        List<int> maximul = new List<int>();
        List<int> minimul = new List<int>();
        // 変曲点
        List<int> inflection = new List<int>();

        double epsilon = 1e-5;
        int steps = 2;

        float[] firstDiff = new float[dataLength];
        float[] secondDiff = new float[dataLength];

        for(int i = 0; i < dataLength; i++)
        {
            int before = i > 0 ? i - 1 : 0;
            int after = i < dataLength - 1 ? i + 1 : dataLength - 1;
            // <!!> 他にいい方法ある？
            firstDiff[i] = velocityData[after] - velocityData[before];
            secondDiff[i] = velocityData[after] - 2 * velocityData[i] + velocityData[before];
        }
        // 極大、極小判定
        for(int i = 0; i < dataLength; i++)
        {
            // ある程度一貫した勾配変化があった場合に認定
            bool consistency = true;
            for(int j = 1; j < steps; j++)
            {
                consistency &= firstDiff[i - j >= 0 ? i - j : 0] * firstDiff[i] >= 0 ? true : false;
                consistency &= firstDiff[i + j >= dataLength ? dataLength - 1 : i + j] * firstDiff[i - j >= 0 ? i - j : 0] <= 0 ? true : false;
            }
            // 勾配から極値認定
            if (consistency)
            {
                if(firstDiff[i] >= 0)
                {
                    maximul.Add(i);
                }
                else
                {
                    minimul.Add(i);
                }
            }
        }
        // 変曲点判定
        for (int i = 0; i < dataLength; i++)
        {
            bool consistency = true;
            for (int j = 1; j < steps; j++)
            {
                consistency &= secondDiff[i - j >= 0 ? i - j : 0] * secondDiff[i] >= 0 ? true : false;
                consistency &= secondDiff[i + j >= dataLength ? dataLength - 1 : i + j] * secondDiff[i - j >= 0 ? i - j : 0] <= 0 ? true : false;
            }
            if (consistency)
            {
                inflection.Add(i);
            }
        }

        // データフィッティング
        // 初期フィット
        List<SubMovement1D> submovements = new List<SubMovement1D>();
        for(int i = 0; i < maximul.Count - 1; i++)
        {
            submovements.Add(new SubMovement1D(velocityData[maximul[i]], maximul[i], (maximul[i + 1] - maximul[i]) * 0.8f));
        }
        /*
        for (int i = 0; i < minimul.Count - 1; i++)
        {
            submovements.Add(new SubMovement1D(velocityData[minimul[i]], minimul[i], (minimul[i + 1] - minimul[i]) * 0.1f));
        }
        
        for (int i = 0; i < inflection.Count - 1; i++)
        {
            // 微妙に検出ミスってる？
            submovements.Add(new SubMovement1D(velocityData[inflection[i]], inflection[i], (inflection[i + 1] - inflection[i]) * 0.1f));
        }
        */
        float[] reconstruct = new float[dataLength];
        // 精度上げ

        maxi = maximul;
        mini = minimul;
        inf = inflection;
        return submovements;
    }

    public List<KeyFrame> Extract(Vector3[] motionData)
    {
        List<KeyFrame> keyframes = new List<KeyFrame>();

        // 中心差分近似で速度に変換して速度→時間の抽出
        int dataLength = motionData.Length;
        float[] velocity = new float[dataLength];
        for(int i = 0; i < dataLength; i++)
        {
            int before = i > 0 ? i - 1 : 0;
            int after = i < dataLength - 1 ? i + 1 : dataLength - 1;
            velocity[i] = (motionData[after] - motionData[before]).magnitude;
        }

        List<int> ma;
        List<int> mi;
        List<int> infl;
        List<SubMovement1D> peakTimes = ExtractFeatures(velocity, out ma, out mi, out infl);

        return keyframes;
    }
}
