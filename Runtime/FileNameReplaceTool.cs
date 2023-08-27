using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;


public class FileNameReplaceTool : UnityEditor.EditorWindow
{
    string folderPath;
    private static float windowWidth = 450;
    private static float windowHeight = 315;
    private static float buttonHeight = 25;
    private List<string> excludedDirectories = new();
    private bool showExcludedDirectories = false;
    private static string toolName = "File Name Replace Tool";
    private static double versionNumber = 1.1;
    private string Keyword = "";
    private string ReplaceWord = "";

    [MenuItem("Tools/File Name Replace Tool")]
    static void Initilize()
    {
        //Window
        FileNameReplaceTool window = (FileNameReplaceTool)EditorWindow.GetWindow(typeof(FileNameReplaceTool), true, string.Format("{0} v{1}", toolName, versionNumber));
        window.position = new Rect(Screen.width, Screen.height - (windowHeight / 2), windowWidth, windowHeight);

        EditorUtility.DisplayDialog("File Name Replace Tool", "The folder with the files you want to rename needs to be located under the assets folder", "Confirm" ); 

        window.Show();        
    }

    void OnGUI()
    {

        // TOP PART
        //_.--.__.-'""`-.__.--.__.-'""`-.__.--.__.-'""`-.__.--.__.-'""`-._
        
        this.Repaint();
        EditorGUI.TextArea(new Rect(0, 5, position.width, 20), string.Format("Directory Path : {0}", GetRelativePath(folderPath)), EditorStyles.largeLabel);
    
        if (GUI.Button(new Rect(0, 25, position.width, buttonHeight), "Select Directory")){
            this.OnFolderSelect();
        }

        EditorGUI.LabelField(new Rect(0, 55, position.width, 20), "Keyword:", EditorStyles.boldLabel);
        Keyword = EditorGUI.TextArea(new Rect(0, 75, position.width, 20), Keyword, EditorStyles.textArea);
        
        EditorGUI.LabelField(new Rect(0, 100, position.width, 20), "Replace word:", EditorStyles.boldLabel);
        ReplaceWord = EditorGUI.TextArea(new Rect(0, 120, position.width, 20), ReplaceWord, EditorStyles.textArea);

        //_.--.__.-'""`-.__.--.__.-'""`-.__.--.__.-'""`-.__.--.__.-'""`-._
        
        // EXCLUDED DIRECTORIES

        var offset = buttonHeight * excludedDirectories.Count;

        showExcludedDirectories = EditorGUI.BeginFoldoutHeaderGroup(new Rect(0, 150, position.width, 10), showExcludedDirectories, "Excluded directories");
        if (showExcludedDirectories){
            for (int i = 0; i < excludedDirectories.Count; i++)
            {
                EditorGUI.LabelField(new Rect(0, 145 + (25 * (i+1)), position.width, 20), excludedDirectories[i], EditorStyles.largeLabel);
            }
        }
        EditorGUI.EndFoldoutHeaderGroup();

        //_.--.__.-'""`-.__.--.__.-'""`-.__.--.__.-'""`-.__.--.__.-'""`-._

        // BOTTOM BUTTONS AND RESIZING

        var AddExcludedDirectory_ButtonY = 175f;
        var Start_ButtonY = 205f;
        var Clear_ButtonY = 235f;
        var Quit_ButtonY = 285f;

        if (showExcludedDirectories){
            AddExcludedDirectory_ButtonY += offset;
            Start_ButtonY += offset;
            Clear_ButtonY += offset;
            Quit_ButtonY += offset;
            this.position = new Rect(this.position.x, this.position.y, windowWidth, 315 + offset);
        }
        else{           
            this.position = new Rect(this.position.x, this.position.y, windowWidth, 315);
        }

        if (GUI.Button(new Rect(0, AddExcludedDirectory_ButtonY, position.width, buttonHeight), "Add excluded directory")){
            this.OnAddExcludedDirectory();
        }

        if (GUI.Button(new Rect(0, Start_ButtonY, position.width, buttonHeight), "Start")){
            this.Start();
        }

        if (GUI.Button(new Rect(0, Clear_ButtonY, position.width, buttonHeight), "Clear")){
            this.Clear();
        }

        if (GUI.Button(new Rect(0, Quit_ButtonY, position.width, buttonHeight), "Quit")){
            this.Close();
        }

        //_.--.__.-'""`-.__.--.__.-'""`-.__.--.__.-'""`-.__.--.__.-'""`-._
    }

    void OnFolderSelect(){
        folderPath = EditorUtility.OpenFolderPanel("Select a directory to walk over", Application.dataPath, "");
    }

    void OnAddExcludedDirectory(){
        excludedDirectories.Add(EditorUtility.OpenFolderPanel("Select a directory to exclude", "", ""));
    }
    
    void Clear(){
        folderPath = "";
        Keyword = "";
        ReplaceWord = "";
        excludedDirectories.Clear();
    }

    void Start(){

        DirectoryInfo dirTop = new DirectoryInfo(folderPath);

        foreach (var file in dirTop.EnumerateFiles("*", SearchOption.AllDirectories))
        {
            if (file.FullName.Contains(Keyword)){ 
                if (!file.FullName.EndsWith(".meta")){
                    string relativepath = GetRelativePath(file.FullName);
                    if (!isChildOfExcludedDirectory(relativepath)){
                        renameFile(file, relativepath);
                    }
                }
                else{
                    //delete all meta files so unity can regenerate them
                    file.Delete();
                }   
            }
        }    

        AssetDatabase.ForceReserializeAssets();
        AssetDatabase.Refresh();
    }

    string GetRelativePath(string fullPath){
        if (fullPath != null && fullPath != ""){
            fullPath = fullPath.Replace("/", "\\");
            string[] pathSplit = fullPath.Split("\\Assets");
            string relativepath = "Assets" + pathSplit[pathSplit.Length-1];
            return relativepath;
        }
        else{
            return "";
        }
    }

    void renameFile(FileInfo file, string relativePath){
        Object asset = AssetDatabase.LoadAssetAtPath<Object>(relativePath);
        if (asset != null)
        {
            AssetDatabase.RenameAsset(relativePath, file.Name.Replace(Keyword, ReplaceWord));
        }
        else{
            file.MoveTo(file.FullName.Replace(Keyword, ReplaceWord));
        }
    }

    bool isChildOfExcludedDirectory(string relativePath){
        bool isChild = false;
        foreach (var dir in excludedDirectories)
        {
            var excludedRelativePath = GetRelativePath(dir);
            if (relativePath.Contains(excludedRelativePath)){
                isChild = true;
            }
        }
        return isChild;
    }
}
