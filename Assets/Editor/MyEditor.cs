using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Test))]
public class MyEditor : Editor {
	
	private List<Vector2> _points; 
	private Test _test;

	private void OnEnable() {
		_test=(Test)target;
		_points=_test.points;
	}

	private void OnSceneGUI() {
		HandleUtility.Repaint();
		Vector2 mousePos=Event.current.mousePosition;
		mousePos=HandleUtility.GUIPointToWorldRay(mousePos).origin;

		int count=_points.Count;
		float nearestLineDistance=1e6f;
		int nearestID=-1;
		Vector2[] nearestLine=new Vector2[2];
		
		for(int i=0;i<count;i++){
			Vector2 p1=_points[i];
			Vector2 p2=_points[(i+1<count)?i+1:0];

			float distance=HandleUtility.DistancePointToLine(mousePos,p1,p2);
			if(distance<nearestLineDistance){
				nearestLineDistance=distance;
				nearestID=i;
				nearestLine[0]=p1;
				nearestLine[1]=p2;
			}
		}

		Handles.lighting=true;
		
		for(int i=0;i<count;i++){
			Vector2 p1=_points[i];
			Vector2 p2=_points[(i+1<count)?i+1:0];
			if(i==nearestID)Handles.color=new Color(0,1,0);
			else Handles.color=new Color(0.5f,1,0.5f);
			Handles.DrawLine(p1,p2);
		}
		
		Vector2 perp=getPerpendicularPt(mousePos.x,mousePos.y,nearestLine[0].x,nearestLine[0].y,nearestLine[1].x,nearestLine[1].y);
		float perpToNearestLineSegment=HandleUtility.DistancePointToLineSegment(perp,nearestLine[0],nearestLine[1]);
		if(perpToNearestLineSegment>0.01f){
			float d0=Vector2.Distance(perp,nearestLine[0]);
			float d1=Vector2.Distance(perp,nearestLine[1]);
			if(d0<d1)perp.Set(nearestLine[0].x,nearestLine[0].y);
			else perp.Set(nearestLine[1].x,nearestLine[1].y);
		}
		_test.point.Set(perp.x,perp.y);

		//绘制文本框
		Handles.Label(_test.point,string.Format("({0},{1})",_test.point.x,_test.point.y));
		
		EditorGUI.BeginChangeCheck();
		float size=HandleUtility.GetHandleSize(_test.point)*0.05f;
		Vector2 snap=Vector2.one*0.05f;
		Vector2 newPoint=Handles.FreeMoveHandle(_test.point,Quaternion.identity,size,snap,Handles.DotHandleCap);
		if(EditorGUI.EndChangeCheck()){
			Undo.RecordObject(_test,"change point");//记录更改，实现撤消回退
			_test.point=newPoint;
		}
		
		//Debug.Log(Event.current);
		
	}

	private Vector2 getPerpendicularPt(float x,float y,float x1,float y1,float x2,float y2){
		//以x1,y1为坐标原点得到向量a，b
		var ax=x-x1;
		var ay=y-y1;
		var bx=x2-x1;
		var by=y2-y1;
		//求向量a,b的点积
		var dot=ax*bx+ay*by;
		//向量b模的平方
		//var bl=Math.sqrt(bx*bx+by*by);
		//var sq=bl*bl;
		//简化
		var sq=bx*bx+by*by;
		//垂点
		var l=dot/sq;
		var ppx=l*bx;
		var ppy=l*by;
		ppx+=x1;
		ppy+=y1;
		//
		return new Vector2(ppx,ppy);

	}

}
