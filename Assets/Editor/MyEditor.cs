using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Test))]
public class MyEditor : Editor {
	
	private const float SnapDistance=10;
	private List<Vector2> _points; 
	private Test _test;

	private int _nearestID=-1;
	private int _editID=-1;
	private Vector2[] _nearestLineSegment=new Vector2[2];

	private void OnEnable() {
		_test=(Test)target;
		_points=_test.points;
	}

	private void OnSceneGUI() {
		if(!_test.enabled)return;
		HandleUtility.Repaint();

		//鼠标位置
		var mousePos=Event.current.mousePosition;
		mousePos=HandleUtility.GUIPointToWorldRay(mousePos).origin;
		
		//寻找最近线段
		findSetNearestLineSegment(mousePos);

		if(Event.current.isMouse){
			EventType eventType=Event.current.type;
			bool isMousePress=false;
			if(Event.current.button==0){
				if(eventType==EventType.mouseDown){
					isMousePress=true;
					var mouseGUI=HandleUtility.WorldToGUIPoint(mousePos);
					var nearestGUI0=HandleUtility.WorldToGUIPoint(_nearestLineSegment[0]);
					var nearestGUI1=HandleUtility.WorldToGUIPoint(_nearestLineSegment[1]);
					//float mouseToNearestLineSegment=HandleUtility.DistancePointToLineSegment(mouseGUI,nearestGUI0,nearestGUI1);
					float d0=Vector2.Distance(mouseGUI,nearestGUI0);
					float d1=Vector2.Distance(mouseGUI,nearestGUI1);
					if(d0<=SnapDistance){
						if(Event.current.control){
							deletePointWithIndex(_nearestID);
						}else{
							_editID=_nearestID;
						}
					}else if(d1<=SnapDistance){
						if(Event.current.control){
							deletePointWithIndex(_nearestID+1);
						}else{
							_editID=_nearestID+1;
						}
					}else{
						Undo.RecordObject(_test,"add point");
						_test.points.Insert(_nearestID+1,new Vector2(mousePos.x,mousePos.y));
						findSetNearestLineSegment(mousePos);
						_editID=_nearestID+1;
					}
				}else if(eventType==EventType.mouseUp){
					isMousePress=false;
					_editID=-1;
				}
			}

			if(!isMousePress){//鼠标没有按下时
				//设置控制柄到最近线段的垂线
				var perp=getPerpendicularPt(mousePos.x,mousePos.y,_nearestLineSegment[0].x,_nearestLineSegment[0].y,_nearestLineSegment[1].x,_nearestLineSegment[1].y);
				var perpGUI=HandleUtility.WorldToGUIPoint(perp);
				var nearestGUI0=HandleUtility.WorldToGUIPoint(_nearestLineSegment[0]);
				var nearestGUI1=HandleUtility.WorldToGUIPoint(_nearestLineSegment[1]);
				float perpToNearestLineSegment=HandleUtility.DistancePointToLineSegment(perpGUI,nearestGUI0,nearestGUI1);
				float d0=Vector2.Distance(perpGUI,nearestGUI0);
				float d1=Vector2.Distance(perpGUI,nearestGUI1);
				//垂足不能滑出线段
				if(perpToNearestLineSegment>0.01f){
					if(d0<d1)perp.Set(_nearestLineSegment[0].x,_nearestLineSegment[0].y);
					else perp.Set(_nearestLineSegment[1].x,_nearestLineSegment[1].y);
				}
				//垂足贴紧端点
				if(d0<d1){
					if(d0<=SnapDistance)perp.Set(_nearestLineSegment[0].x,_nearestLineSegment[0].y);
				}else{
					if(d1<=SnapDistance)perp.Set(_nearestLineSegment[1].x,_nearestLineSegment[1].y);
				}
				_test.point.Set(perp.x,perp.y);
			}
		}
		
		editPointHandler();
		//画点列表
		drawPoints();
	}

	private void deletePointWithIndex(int index){
		if(_test.points.Count<3)return;
		Undo.RecordObject(_test,"delete point");
		_test.points.RemoveAt(index);
	}

	private void editPointHandler(){
		Handles.Label(_test.point,string.Format("({0},{1})",_test.point.x,_test.point.y));
		EditorGUI.BeginChangeCheck();
		float size=HandleUtility.GetHandleSize(_test.point)*0.05f;
		var snap=Vector2.one*0.05f;
		var newPoint=Handles.FreeMoveHandle(_test.point,Quaternion.identity,size,snap,Handles.DotHandleCap);
		if(EditorGUI.EndChangeCheck()){
			Undo.RecordObject(_test,"edit point");//记录更改，实现撤消回退
			_test.point=newPoint;
			if(_editID>-1){
				_test.points[_editID]=newPoint;
			}
		}
	}

	private void findSetNearestLineSegment(Vector2 refPoint){
		int count=_points.Count;
		float nearestLineDistance=1e6f;
		for(int i=0;i<count;i++){
			var p1=_points[i];
			var p2=_points[(i+1<count)?i+1:0];
			float distance=HandleUtility.DistancePointToLine(refPoint,p1,p2);
			if(distance<nearestLineDistance){
				nearestLineDistance=distance;
				_nearestID=i;
				_nearestLineSegment[0]=p1;
				_nearestLineSegment[1]=p2;
			}
		}
	}

	private void drawPoints(){
		int count=_points.Count;
		for(int i=0;i<count;i++){
			Handles.Label(_points[i],string.Format("{0}",i));
			var p1=_points[i];
			var p2=_points[(i+1<count)?i+1:0];
			if(i==_nearestID){
				Handles.color=new Color(0,1,0);
			}else{
				Handles.color=new Color(0.5f,1,0.5f);
			}
			Handles.DrawLine(p1,p2);
		}
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
