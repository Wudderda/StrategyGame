using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
//* get closest unitte hareket eden unitlere formation yaparken null exception yiyor cnkü has pathi cekliyorum yeterince asker çkmıo
public class Game_Controller : MonoBehaviour
{
    // Start is called before the first frame update
    RaycastHit hit;
    Ray ray;
    //GameObject Selected_Unit;
    List<GameObject> All_Units= new List<GameObject>();
    List<GameObject> Selected_Units;
    public RectTransform box;
    public GameObject debug_soldier;
    public float flanking_distance;
    public float flanking_degree;
    bool is_dragging;
    Vector2 size;
    Vector3 position;
    Vector3 last_clicked_position;
    Vector3 mid_point;// this is the average position of selected units with box selection
    Vector3 Slot;
    //LayerMask mask;
    public void AddUnit(GameObject unit)
    {
        All_Units.Add(unit);
    }
    public void DeleteUnit(GameObject unit)
    {
        All_Units.Remove(unit);
    }
    private GameObject getClosestUnit(Vector3 slot) // finds the closest non-moving selected unit  to the slot 
    {
        double min = double.MaxValue; // Closest unit so far
        GameObject res = null;
        for (int i=0;i<Selected_Units.Count;i++)
        {
           if(Vector3.Distance(Selected_Units[i].transform.position, slot)<min && !Selected_Units[i].GetComponent<Unit>().hasPath())
            {
                min = Vector3.Distance(Selected_Units[i].transform.position, slot);
                res = Selected_Units[i];
            }
        }
        return res;
    }
    void Start()
    {
        //mask = LayerMask.GetMask("Wa");
        Selected_Units = new List<GameObject>();
        is_dragging = false;
        box.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A) && Selected_Units.Count>0)
        {
            for (int i=0; i<Selected_Units.Count;i++)
            {
                Selected_Units[i].GetComponent<Unit>().SetState(Unit.State.ATTACK);

            }
        }
        if (Input.GetMouseButtonDown(1) && Selected_Units.Count>0)//formation Code 
        {
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Physics.Raycast(ray, out hit,Mathf.Infinity,-1,QueryTriggerInteraction.Ignore);
            float offset = 25f;// this can change for each unit for different sizes
            set_mid_point();
            Vector3 dir = hit.point - mid_point;// moving direction used to offset for positioning some units to the front
            dir.y = 0;
            dir = dir.normalized;
            Vector3 line_vector;

            line_vector = (Vector3.Cross(dir, Vector3.up)).normalized; //forming direction this is the left side of clicked position
            
            for (int i= 0; i < Selected_Units.Count;i++)
            {
                if (Input.GetKey(KeyCode.Alpha2) ) //two line formation
                {
                    if (i < Selected_Units.Count / 2)// front line 
                    {
                        Slot = hit.point + (i) * offset * line_vector  + (Selected_Units.Count / 2) / 2 * -line_vector*offset;
       
                    }
                    else//back line
                    {
                        Slot = hit.point + (i- Selected_Units.Count / 2 ) * offset * line_vector+ (Selected_Units.Count/2+1)/2*-line_vector*offset - offset * dir;
                    }
                    if (getClosestUnit(Slot)!=null)
                    {
                        getClosestUnit(Slot).GetComponent<Unit>().Move_to(Slot);
                    }
                }
                else if (Input.GetKey(KeyCode.F))//Flank Positioning
                {
                    Vector3 flank1 = Quaternion.Euler(0, -flanking_degree, 0) * -dir; //flanking angles
                    Vector3 flank2 = Quaternion.Euler(0, flanking_degree, 0) * -dir;
                    if (i % 2 == 0)
                    {
                        Slot = hit.point + offset * (i / 2 + 1) * flank1 - line_vector * flanking_distance;
                    }
                    else
                    {
                        Slot = hit.point + offset * (i / 2 + 1) * flank2 + line_vector * flanking_distance;
                    }
                    getClosestUnit(Slot).GetComponent<Unit>().Move_to(Slot);
                }
                else// units has no formation which means they will travel in the same positioning with direction allignment
                {
                    Selected_Units[i].GetComponent<Unit>().Move_to(hit.point - mid_point + Selected_Units[i].transform.position);
                }
            }

        }
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetMouseButtonDown(0)) // selection with shift
        {
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Physics.Raycast(ray, out hit, Mathf.Infinity, -1, QueryTriggerInteraction.Ignore);
            if (hit.collider.gameObject.tag == "Unit" && !Selected_Units.Contains(hit.collider.gameObject))
            {
                hit.collider.gameObject.GetComponent<Unit>().select();
                Selected_Units.Add(hit.collider.gameObject);
                All_Units.Add(hit.collider.gameObject);
            }
        }

        else if (Input.GetMouseButtonDown(0)) // leftclick selection(single unit) or setting of box selection variables
        {
            last_clicked_position = Input.mousePosition;
            is_dragging = true;
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Physics.Raycast(ray, out hit, Mathf.Infinity, -1, QueryTriggerInteraction.Ignore);
            if (hit.collider.gameObject.tag == "Unit")
            {//left clicked on unit
                if (Selected_Units.Count==0)
                {
                    hit.collider.gameObject.GetComponent<Unit>().select();
                    Selected_Units.Add(hit.collider.gameObject);
                }
                else
                {
                    for (int i = 0; i < Selected_Units.Count; i++)
                    {
                        Selected_Units[i].GetComponent<Unit>().deselect();
                    }
                    Selected_Units.Clear();
                    Selected_Units.Add(hit.collider.gameObject);
                    hit.collider.gameObject.GetComponent<Unit>().select();
                }

            }
            else
            {//left clicked on terrain
                if (Selected_Units.Count != 0) {
                    for(int i=0; i< Selected_Units.Count; i++)
                    {
                        Selected_Units[i].GetComponent<Unit>().deselect();
                    }
                    Selected_Units.Clear();
                }
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            is_dragging = false;
            box.gameObject.SetActive(false);
        }
        else if (Input.GetMouseButton(0)) { // rectangle creation
            if (is_dragging && Vector3.Distance( Input.mousePosition,last_clicked_position)>=5f)
            {// Player is dragging 
                size = new Vector2((Mathf.Abs(Input.mousePosition.x- last_clicked_position.x)) , (Mathf.Abs(Input.mousePosition.y - last_clicked_position.y)));
                position =(Input.mousePosition + last_clicked_position)/2 ;
                box.position = position;
                box.sizeDelta = size;
                float x_pos1 = position.x-size.x/2;
                float x_pos2 = position.x + size.x/2;
                float y_pos1 = position.y - size.y/2;
                float y_pos2 = position.y + size.y/2;
                if (!box.gameObject.activeInHierarchy)
                {
                    box.gameObject.SetActive(true);
                }
                foreach(GameObject unit in All_Units)
                {
                    Vector3 unit_screen_pos = Camera.main.WorldToScreenPoint(unit.transform.position);
                    if((unit_screen_pos.x> x_pos1 && unit_screen_pos.x > x_pos2) || (unit_screen_pos.x < x_pos1 && unit_screen_pos.x < x_pos2)) // no match on x axis
                    {
                        Selected_Units.Remove(unit);
                        unit.GetComponent<Unit>().deselect();
                        continue;
                    }
                    else//x axis matched
                    {
                        if ((unit_screen_pos.y > y_pos1 && unit_screen_pos.y > y_pos2) || (unit_screen_pos.y < y_pos1 && unit_screen_pos.y < y_pos2)) // no match on y axis
                        {
                            Selected_Units.Remove(unit);
                            unit.GetComponent<Unit>().deselect();
                            continue;
                            
                        }
                        else// we are inside the rectangle
                        {
                            if (!Selected_Units.Contains(unit))
                            {
                                Selected_Units.Add(unit);
                               unit.GetComponent<Unit>().select();
                            }
                        }

                    }
                }
            }
        }

    }
    void set_mid_point()// sets the mid_point variable which contains average positions of selected units
    {
        mid_point = Vector3.zero;
        foreach(GameObject unit in Selected_Units)
        {
            mid_point += unit.transform.position;
        }
        mid_point = mid_point / Selected_Units.Count;
    }
}
