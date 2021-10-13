////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/// <summary>                                                                                                        ///
/// Colafe findre module used to find components and gameobjects by their name.                                      ///
/// </summary>                                                                                                       ///
/// <note>                                                                                                           ///
/// Author: Luciano A. Candal                                                                                        ///
/// License: Mozilla Public License 2.0 <href>https://github.com/Lucinotion/ColafeScript/blob/main/LICENSE</href>    ///
/// Repository: This code is available at <href>https://github.com/Lucinotion/ColafeScript</href>                    ///
/// </note>                                                                                                          ///
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using ColafeScript.Utils;

namespace ColafeScript.Finder
{
    /// <summary>
    /// Class with functions to find objects and components by name, path and type.
    /// </summary>
    public static class Find{
        static readonly char[] separator = new char[] { '/' };

        /// <summary>
        /// Finds all objects matching the expression.
        /// </summary>
        /// <param name="exp">
        /// A path expression using / with * at the start for
        /// selecting all the objects, * at the end for selecting all siblings,
        /// /... at the end to mach the first child or in the middle to indicate
        /// that the path is incomplete.
        /// </param>
        /// <returns>The list of found objects or null if none is found.</returns>
        public static List<GameObject> FindObjects(string exp){
            List<GameObject> foundObjects;
            bool findAllObjects = exp.First() == '*';
            bool findAllSiblings = exp.Last() == '*';
            exp = exp.Trim('*');
            if(findAllObjects){
                foundObjects = FindAllInstancesOf(exp);
                if(foundObjects != null){
                    if(findAllSiblings)
                        return FindAllSiblingsOf(foundObjects);
                    else
                        return foundObjects;
                }
            }
            else{
                GameObject foundObject = FindFirstInstanceOf(exp);
                if(foundObject != null){
                    if(findAllSiblings)
                        return FindAllSiblingsOf(foundObject);
                    else{
                        foundObjects = new List<GameObject>();
                        foundObjects.Add(foundObject);
                        return foundObjects;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Finds all components of a type matching a expression.
        /// </summary>
        /// <param name="exp">The type to search, adding a * means finds all, omission of * finds the first instance.</param>
        /// <returns>A list containing the components found, null if not found.</returns>
        public static List<Component> FindComponents(string exp){
            bool findAllComponents = exp.Last() == '*' || exp.First() == '*';
            exp = exp.Trim('*');
            System.Type componentType = System.Type.GetType(exp);
            List<Component> components = null;
            if(findAllComponents)
                components = FindAllComponentsOfType(componentType);
            else{
                Component foundComponent = FindFirstComponentOfType(componentType);
                if(foundComponent != null){
                    components = new List<Component>();
                    components.Add(foundComponent);
                }
            }
            return components;
        }

        /// <summary>
        /// Finds components of a type in a list of gameobjects using a expression.
        /// Adding a * means find all, omission of * means find only the first instances in each object.
        /// </summary>
        /// <param name="objects">The list of gameobjects to seach in.</param>
        /// <param name="exp"></param>
        /// <returns></returns>
        public static List<Component> FindComponentsInObjects(List<GameObject> objects, string exp){
            bool findAllComponents = exp.Last() == '*' || exp.First() == '*';
            exp = exp.Trim('*');
            System.Type componentType = System.Type.GetType(exp);
            List<Component> components;
            if(findAllComponents)
                components = FindAllComponentsOfTypeInObjects(componentType, objects);
            else
                components = FindFirstComponentsOfTypeInObjects(componentType, objects);
            return components;
        }

        /// <summary>
        /// Finds the first components instance of a type in a list of gameobjects.
        /// </summary>
        /// <param name="componentType">The type of component to find.</param>
        /// <param name="gameObjects">The list of gameobjects to seach in.</param>
        /// <returns>A list of all found components of that type in the list, null if none is found.</returns>
        public static List<Component> FindFirstComponentsOfTypeInObjects(System.Type componentType, List<GameObject> gameObjects){
            List<Component> componentList = new List<Component>();
            Component component;
            foreach (GameObject go in gameObjects){
                component = go.GetComponent(componentType);
                if(component != null)
                    componentList.Add(component);
            }
            if(componentList.Count > 0)
                return componentList;
            else
                return null;
        }

        /// <summary>
        /// Finds all the components of a specific type in a list of gameobjects.
        /// </summary>
        /// <param name="componentType">The type of component to find.</param>
        /// <param name="gameObjects">The list of gameobjects to seach in.</param>
        /// <returns>A list of all found components of that type in the list, null if none is found.</returns>
        public static List<Component> FindAllComponentsOfTypeInObjects(System.Type componentType, List<GameObject> gameObjects){
            List<Component> componentList = new List<Component>();
            Component[] components;
            foreach (GameObject go in gameObjects){
                components = go.GetComponents(componentType);
                if(components != null)
                    foreach (Component component in components)
                        componentList.Add(component);
            }
            if(componentList.Count > 0)
                return componentList;
            else
                return null;
        }

        /// <summary>
        /// Finds the first component of a specific type.
        /// </summary>
        /// <param name="componentType">The type of component to find.</param>
        /// <returns>First component of that type, null if not found.</returns>
        public static Component FindFirstComponentOfType(System.Type componentType){
            Component component = GameObject.FindObjectOfType(componentType) as Component;
            if(component != null)
                return component;
            else
                return null;
        }

        /// <summary>
        /// Returns a list of all components of a specific type.
        /// </summary>
        /// <param name="componentType">The type of component to find.</param>
        /// <returns>List of all components of that type, null if not found.</returns>
        public static List<Component> FindAllComponentsOfType(System.Type componentType){
            List<Component> components = new List<Component>();
            var foundComponents = GameObject.FindObjectsOfType(componentType);
            if(foundComponents != null)
                return foundComponents.Cast<Component>().ToList();
            else
                return null;
        }

        /// <summary>
        /// Finds the first instance of a GameObject inside a given path, even if it's disabled.
        /// </summary>
        /// <param name="exp">The path or name of the object to find (use / for the path).</param>
        /// <returns>A GameObject if found, null otherwise</returns>
        public static GameObject FindFirstInstanceOf(string exp){
            if(CheckErrorNullOrEmptyPath(exp))
                return null;
            GameObject returnObject = null;
            if(exp.Contains("/...")){
                string[] pathContents = exp.Split(separator, System.StringSplitOptions.RemoveEmptyEntries);
                if (pathContents.Length == 2)                                   // Objects/... case
                    returnObject = FindFirstChildInObject(pathContents[0]);
                else if (pathContents.Last() == "...")                          // Objects/Object/... case
                    returnObject = FindFirstChildInPath(exp.Substring(0, exp.LastIndexOf('/')));
                else{                                                           // Objects/Object/.../ObjName case
                    string[] splitPaths = exp.Split(new[] { "/.../" }, System.StringSplitOptions.None);
                    returnObject = FindFirstObjectInIncompletePath(splitPaths[0], splitPaths.Last());
                }
            }
            else                                                                // Object or Objects/Object case
                returnObject = GameObject.Find(exp);
            if (returnObject != null)
                return returnObject;
            else
                Msg.Wrn("ColafeScript: Expression '" + exp + "' found no matching object.");
            return null;
        }

        /// <summary>
        /// Finds all instances of GameObjects inside a given path, even if it's disabled.
        /// </summary>
        /// <param name="exp">The path or name of the objects to find (use / for the path).</param>
        /// <returns>A list of the GameObjects if any is found, null otherwise</returns>
        public static List<GameObject> FindAllInstancesOf(string exp){
            if(CheckErrorNullOrEmptyPath(exp))
                return null;
            List<GameObject> returnObjects = null;
            if(exp.Contains("/...")){
                string[] pathContents = exp.Split(separator, System.StringSplitOptions.RemoveEmptyEntries);
                if(pathContents.Length == 2)                                    // Parent/... case
                    returnObjects = FindFirstChildInObjects(pathContents[0]);
                else if(pathContents.Last() == "...")                           // Grandparent/Parent/... case
                    returnObjects = FindFirstChildInPaths(exp.Substring(0, exp.LastIndexOf('/')));
                else{ // Parent/Child/.../ObjectName case
                    string[] splitPaths = exp.Split(new[] { "/.../" }, System.StringSplitOptions.None);
                    returnObjects = FindAllObjectsInIncompletePath(splitPaths[0], splitPaths[1]); 
                }
            }
            else{
                bool isPath = exp.Contains('/');
                if(isPath)                                                      // Parent/Child case
                    returnObjects = FindAllObjectsWithPath(exp);
                else                                                            // ObjectsName case
                    returnObjects = FindAllObjectsWithName(exp);
            }
            if (returnObjects != null && returnObjects.Count() > 0)
                return returnObjects;
            else
                Msg.Wrn("ColafeScript: Expression '" + exp + "' found no matching objects.");
            return null;
        }

        /// <summary>
        /// Finds all siblings of a gameobject including the gameobject itself.
        /// </summary>
        /// <param name="go">The gameobject to find the siblings of.</param>
        /// <returns>A list of all the gameobject's siblings including itself.</returns>
        public static List<GameObject> FindAllSiblingsOf(GameObject go){
            List<GameObject> siblings = new List<GameObject>();
            foreach (Transform sibling in go.transform.parent)
                siblings.Add(sibling.gameObject);
            return siblings;
        }

        /// <summary>
        /// Finds all siblings of all gameobjects in a list, including the gameobjects too.
        /// </summary>
        /// <param name="gos">The gameobjects to find the siblings of.</param>
        /// <returns>A list of all the gameobjects siblings including themselves/</returns>
        public static List<GameObject> FindAllSiblingsOf(List<GameObject> gos){
            List<GameObject> siblings = new List<GameObject>();
            foreach (var child in gos)
                foreach (Transform sibling in child.transform.parent)
                    siblings.Add(sibling.gameObject);
            return siblings;
        }

        /// <summary>
        /// Returns a string containing the path to the object in the scene.
        /// </summary>
        /// <param name="objTransform">The object to find the path to.</param>
        /// <param name="includeObjectNameInPath">Include the name of the object at the end of the path?</param>
        /// <returns>The path to the object in a /Parent/Child/ format.</returns>
        public static string GetPathTo(Transform objTransform, bool includeObjectNameInPath = false){
            StringBuilder concatPath = new StringBuilder();
            Transform parent = includeObjectNameInPath ? objTransform : objTransform.parent;
            while(parent != null){
                concatPath.Insert(0, parent.name);
                concatPath.Insert(0, '/');
                parent = parent.parent;
            }
            return concatPath.ToString() + '/';
        }

#region Get All and Get First Named Objects

        static Transform GetFirstParentNamed(string objName){
            foreach (var parent in GameObject.FindObjectsOfType<Transform>().Where(tr => tr.childCount > 0))
                if(parent.name == objName)
                    return parent;
            return null;
        }

        static IEnumerable<Transform> GetAllObjectsNamed(string objName){
            return GameObject.FindObjectsOfType<Transform>().Where(
                tr => tr.name == objName
            );
        }

        static IEnumerable<Transform> GetAllParentObjectsNamed(string parentName){
            return GameObject.FindObjectsOfType<Transform>().Where(
                tr => tr.name == parentName && tr.childCount > 0
            );
        }

        static IEnumerable<Transform> GetAllChildObjectsNamed(string childName){
            return GameObject.FindObjectsOfType<Transform>().Where(
                tr => tr.name == childName && tr.parent != null
            );
        }

        static IEnumerable<Transform> GetAllChildparentsNamed(string parentchildName){
            return GameObject.FindObjectsOfType<Transform>().Where(
                tr => tr.name == parentchildName && tr.childCount > 0 && tr.parent != null
            );
        }

#endregion

#region Find All and Find First Object/Path

        static List<GameObject> FindAllObjectsWithName(string path){
            List<GameObject> returnObjects = new List<GameObject>();
            foreach (var obj in GetAllObjectsNamed(path))
                returnObjects.Add(obj.gameObject);
            return returnObjects;
        }

        static List<GameObject> FindAllObjectsWithPath(string path){
            List<GameObject> returnObjects = new List<GameObject>();
            string childName = path.Substring(path.LastIndexOf('/') + 1);
            string subPath = '/' + path.Substring(0, path.LastIndexOf('/') + 1);
            foreach (var child in GetAllChildObjectsNamed(childName))
                if (GetPathTo(child).Contains(subPath))
                    returnObjects.Add(child.gameObject);
            return returnObjects;
        }

        static List<GameObject> FindAllObjectsInIncompletePath(string parentPath, string objectName){
            List<GameObject> returnObjects = new List<GameObject>();
            string subPath = '/' + parentPath + '/';
            foreach (Transform child in GetAllChildObjectsNamed(objectName))
                if (GetPathTo(child).Contains(subPath))
                    returnObjects.Add(child.gameObject);
            return returnObjects;
        }

        static GameObject FindFirstChildInPath(string path){
            string childparentName = path.Substring(path.LastIndexOf('/') + 1);
            string subPath = '/' + path.Substring(0, path.LastIndexOf('/') + 1);
            foreach (Transform child in GetAllChildparentsNamed(childparentName))
                if (GetPathTo(child).Contains(subPath))
                    return child.GetChild(0).gameObject;
            return null;
        }

        static List<GameObject> FindFirstChildInPaths(string path){
            List<GameObject> returnObjects = new List<GameObject>();
            string childparentName = path.Substring(path.LastIndexOf('/') + 1);
            string subPath = '/' + path.Substring(0, path.LastIndexOf('/') + 1);
            foreach (Transform child in GetAllChildparentsNamed(childparentName))
                if (GetPathTo(child).Contains(subPath))
                    returnObjects.Add(child.GetChild(0).gameObject);
            return returnObjects;
        }
        
        static GameObject FindFirstChildInObject(string parentName){
            GameObject returnObject = null;
            var parent = GetFirstParentNamed(parentName);
            if (parent != null)
                returnObject = parent.GetChild(0).gameObject;
            return returnObject;
        }

        static List<GameObject> FindFirstChildInObjects(string objName){
            List<GameObject> returnObjects = new List<GameObject>();
            foreach (Transform parent in GetAllParentObjectsNamed(objName))
                returnObjects.Add(parent.GetChild(0).gameObject);
            return returnObjects;
        }

        static GameObject FindFirstObjectInIncompletePath(string parentPath, string objName){
            string subPath = '/' + parentPath + '/';
            foreach (var child in GetAllChildObjectsNamed(objName))
                if (GetPathTo(child).Contains(subPath))
                    return child.gameObject;
            return null;
        }


#endregion

#region Error checking

        static bool CheckErrorNullOrEmptyPath(string path){
            if(string.IsNullOrEmpty(path)){
                Msg.Err("ColafeScript: Objectname/path is empty or null.");
                return true;
            }
            return false;
        }

#endregion
    }
}