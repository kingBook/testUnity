﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Test))]
public class MyEditor : Editor {
	
	private void OnSceneGUI() {
		Test test=(Test)target;
		
		//绘制文本框
		/*Handles.Label(test.point,string.Format("({0},{1})",test.point.x,test.point.y));
		
		EditorGUI.BeginChangeCheck();
		float size=HandleUtility.GetHandleSize(test.point)*0.05f;
		Vector2 snap=Vector2.one*0.05f;
		Vector2 newPoint=Handles.FreeMoveHandle(test.point,Quaternion.identity,size,snap,Handles.DotHandleCap);
		if(EditorGUI.EndChangeCheck()){
			Undo.RecordObject(test,"change point");//记录更改，实现撤消回退
			test.point=newPoint;
		}*/

		Vector2 mousePos=Event.current.mousePosition;
		
		Debug.LogFormat("mosuePos:{0}",mousePos);
		
		for(int i=0;i<test.points.Count;i++){
			Vector2 p1=test.points[i];
			Vector2 p2=test.points[(i+1<test.points.Count)?i+1:0];
			Handles.color=new Color(0.5f,1,0.5f);
			if(i==0){
				//Debug.Log("p1:"+HandleUtility.WorldToGUIPoint(p1));
				Debug.Log(HandleUtility.DistancePointToLine(mousePos,HandleUtility.WorldToGUIPoint(p1),HandleUtility.WorldToGUIPoint(p2)));
			}
			Handles.DrawLine(p1,p2);
			
		}
		
		
		
	}

	

}
