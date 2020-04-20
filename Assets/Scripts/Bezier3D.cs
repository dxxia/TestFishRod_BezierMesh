using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bezier3D : MonoBehaviour
{
    //控制点
    public Transform[] cpArr;
    //施力物体
    public ForceItem forceItem;

    //原顶点位置
    private Vector3[] oriVertices;

    private Mesh m_Mesh;
    private Vector3 topPos;//最后一个控制点的位置。用来计算mesh高度来计算t

    void Start()
    {
        topPos = cpArr[cpArr.Length - 1].TransformPoint(new Vector3(0, 0, 0));//顶部坐标
        topPos = cpArr[0].InverseTransformPoint(topPos);//相对p0坐标

        m_Mesh = GetComponent<MeshFilter>().mesh;

        oriVertices = (Vector3 [])m_Mesh.vertices.Clone();
        //转换成p0的相对坐标
        for(int i = 0; i < oriVertices.Length; i++)
        {
            oriVertices[i] = transform.TransformPoint(oriVertices[i]);//世界坐标
            oriVertices[i] = cpArr[0].InverseTransformPoint(oriVertices[i]);//相对p0坐标
        }
    }

    void Update()
    {   
        //根据受力，修改控制点
        UpdateControlPoint();
        //更新mesh
        UpdateBezierBend();
    }

    /********************************贝塞尔曲线Mesh计算相关*********************************/
    // 对原来的顶点做贝塞尔曲线变换，得到弯曲变换后对应的点位置
    private void UpdateBezierBend()
    {   
        //判断曲线弯曲方向
        Vector3 bendVector = new Vector3(0, 0, 0);
        bool isVertical = true;
        for(int i = 1; i < cpArr.Length; i++)
        {
            Vector3 pos = cpArr[i].TransformPoint(new Vector3(0, 0, 0));
            pos = cpArr[0].InverseTransformPoint(pos);
            if(IsEqualZero(pos.x) == false || IsEqualZero(pos.z) == false)
            {
                bendVector.x = pos.x;
                bendVector.z = pos.z;
                isVertical = false;
                break;
            }
        }

        Vector3[] temp = (Vector3 [])m_Mesh.vertices.Clone();
        for(int i = 0; i < oriVertices.Length; i++)
        {
            //获取顶点坐标,计算t值
            Vector3 oriPos = oriVertices[i];
            Vector3 bendPos;
            if(isVertical == true)
            {
                bendPos = oriPos;
            }
            else
            {
                float t = oriPos.y / topPos.y;
                //获取顶点在贝塞尔曲线上对应的坐标
                Vector3 bezierPos = CalculateBezier(t); 
                //获取顶点在曲线上应有的法线偏移向量
                Vector3 normalVector = GetBendNormalVector(t, oriPos, bendVector); 
                //获取顶点在曲线上应有的垂直偏移向量
                Vector3 verticalVector = new Vector3(oriPos.x, 0, oriPos.z) - Vector3.Project(new Vector3(oriPos.x, 0, oriPos.z), bendVector); 
                //获取顶点最终弯曲位置
                bendPos = bezierPos + normalVector + verticalVector;
            }
            //转换回mesh本地坐标系
            bendPos = cpArr[0].TransformPoint(bendPos);
            bendPos = transform.InverseTransformPoint(bendPos);
            temp[i] = bendPos;
        }
        m_Mesh.vertices = temp;
    }

    //bezier曲线公式
    private Vector3 CalculateBezier(float t)
    {
        Vector3 ret = new Vector3(0, 0, 0);
        int n = cpArr.Length - 1;

        for(int i = 0; i <= n; i++)
        {
            Vector3 pi = cpArr[i].TransformPoint(new Vector3(0, 0, 0));
            pi = cpArr[0].InverseTransformPoint(pi);

            ret = ret + Mathf.Pow(1 - t, n - i) * Mathf.Pow(t, i) * Cn_m(n, i) * pi;
        }

        return ret;
    }

    //曲线求导（切线向量）
    private Vector3 CalculateBezierTangent(float t)
    {
        Vector3 ret = new Vector3(0, 0, 0);
        int n = cpArr.Length - 1;

        for(int i = 0; i <= n; i++)
        {
            Vector3 pi = cpArr[i].TransformPoint(new Vector3(0, 0, 0));
            pi = cpArr[0].InverseTransformPoint(pi);

            ret = ret + (-1 * (n - i) * Mathf.Pow(1 - t, n - i - 1) * Mathf.Pow(t, i) * Cn_m(n, i) * pi + i * Mathf.Pow(1 - t, n - i) * Mathf.Pow(t, i - 1) * Cn_m(n, i) * pi);
        }

        return ret;
    }

    // 获取指定点上的法向量偏移
    private Vector3 GetBendNormalVector(float t, Vector3 oriPos, Vector3 bendVector)
    {
        Vector3 tangentVector = CalculateBezierTangent(t);//切线斜率
        //切线竖直时，顶点在在弯曲向量上的投影向量即为法向量
        if(IsEqualZero(tangentVector.x) == true && IsEqualZero(tangentVector.z) == true)
        {
            return Vector3.Project(new Vector3(oriPos.x, 0, oriPos.z), bendVector);
        }

        Vector3 normalVector = new Vector3(0, 0, 0);
        float directFlag = Vector3.Dot(bendVector, oriPos);
        //判断法向量朝向（法向量有两个方向）
        if(directFlag > 0)//顶点坐标与弯曲方向同向
        {
            if(IsEqualZero(tangentVector.y) == true)//切线水平，法向量竖直向下
            {
                normalVector.y = -1;
            }
            else
            {
                if(tangentVector.y > 0)//切线朝上，法向量与切线水平同向
                {
                    normalVector.x = tangentVector.x;
                    normalVector.z = tangentVector.z;
                }
                else//切线朝下，法向量与切线水平反向
                {
                    normalVector.x = -tangentVector.x;
                    normalVector.z = -tangentVector.z;
                }
                normalVector.y = -(tangentVector.x * normalVector.x + tangentVector.z * normalVector.z )/tangentVector.y;
            }
        }
        else//顶点坐标与弯曲方向反向
        {
            if(IsEqualZero(tangentVector.y) == true)//切线水平，法向量竖直向上
            {
                normalVector.y = 1;
            }
            else
            {
                if(tangentVector.y > 0)//切线朝上，法向量与切线水平反向
                {
                    normalVector.x = -tangentVector.x;
                    normalVector.z = -tangentVector.z;
                }
                else//切线朝下，法向量与切线水平同向
                {
                    normalVector.x = tangentVector.x;
                    normalVector.z = tangentVector.z;
                }
                normalVector.y = -(tangentVector.x * normalVector.x + tangentVector.z * normalVector.z )/tangentVector.y;
            }
        }

        //法向量的模应为到投影到弯曲面后，到中心点的距离
        float magnitude = Vector3.Project(new Vector3(oriPos.x, 0, oriPos.z), bendVector).magnitude;
        normalVector = normalVector.normalized * magnitude;

        return normalVector;
    }

    //浮点判断是否为零
    private bool IsEqualZero(float value)
    {
        return Mathf.Abs(value) < 1e-5;
    }

    //组合数
    private int Cn_m(int n, int m)
    {
        int ret = 1;
        for(int i = 0; i < m; i++){
            ret = ret * (n - i) / (i + 1);  
        }
        return ret;    
    }

    /************************************根据受力情况计算控制点坐标(旋转)*****************************/
    private void UpdateControlPoint()
    {
        float handleForce = forceItem.force;
        //根据受力计算各个控制点旋转角度
        for(int i = 1; i <= cpArr.Length - 2; i++)
        {
            //计算最大弯曲方向
            Vector3 forcePos = forceItem.transform.TransformPoint(new Vector3(0, 0, 0));
            forcePos = cpArr[i - 1].InverseTransformPoint(forcePos);
            Vector3 toVector = forcePos - cpArr[i].localPosition;
            Quaternion maxRotation =  Quaternion.FromToRotation(Vector3.up, toVector);
            //计算弯曲比例
            ControlPoint cp = cpArr[i].gameObject.GetComponent<ControlPoint>();
            float rotateRate = Mathf.Clamp(handleForce / cp.bendForce, 0f, 1.0f);
            //设置旋转角度
            cpArr[i].localRotation = Quaternion.Lerp(Quaternion.Euler(0, 0, 0), maxRotation, rotateRate);
        }
    }
}
