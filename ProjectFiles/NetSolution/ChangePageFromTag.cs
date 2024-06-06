#region Using directives

using System;
using System.Collections.Generic;
using FTOptix.HMIProject;
using FTOptix.NetLogic;
using FTOptix.UI;
using UAManagedCore;

#endregion

public class ChangePageFromTag : BaseNetLogic
{
    public override void Start()
    {
        // Get the PanelLoader object
        panelLoader = InformationModel.Get<PanelLoader>(LogicObject.GetVariable("PanelLoader").Value);
        // Get the PageChangeTag variable
        pageChangeTag = InformationModel.GetVariable(LogicObject.GetVariable("PageChangeTag").Value);
        // Create a new LongRunningTask object to index all pages
        myLongRunningTask = new LongRunningTask(CreatePagesList, LogicObject);
        // Start the LongRunningTask
        myLongRunningTask.Start();
        // Trigger the CurrentPanel_VariableChange method to set the initial page
        CurrentPanel_VariableChange(null, null);
    }

    public override void Stop() =>
        // Insert code to be executed when the user-defined logic is stopped
        myLongRunningTask.Dispose();

    [ExportMethod]
    public void CreatePagesList()
    {
        // Blank current dictionary of screens
        pagesDictionary.Clear();
        // Get the ScreensFolder variable
        NodeId screensFolder = LogicObject.GetVariable("ScreensFolder").Value;
        // Get a list of all screens with a "ScreenId" variable
        RecursiveSearch(InformationModel.Get(screensFolder));
        // Subscribe to the PageChangeTag variable
        pageChangeTag.VariableChange += PageChangeTag_VariableChange;
        // Subscribe to the CurrentPanel variable
        panelLoader.GetVariable("CurrentPanel").VariableChange += CurrentPanel_VariableChange;
    }

    private void PageChangeTag_VariableChange(object sender, VariableChangeEventArgs e)
    {
        int newValue;
        try
        {
            newValue = pageChangeTag.Value;
        }
        catch
        {
            newValue = 0;
        }

        if (newValue > 0)
        {
            var pageExists = pagesDictionary.TryGetValue((uint)newValue, out var destinationPage);

            if (!pageExists)
                Log.Warning("PageChangeTag", "Page with ID [" + pageChangeTag.Value + "] does not exist");
            else
                panelLoader.ChangePanel(InformationModel.Get(destinationPage));
        }
    }

    private void CurrentPanel_VariableChange(object sender, VariableChangeEventArgs e)
    {
        var currentPanel = InformationModel.Get(panelLoader.CurrentPanel);
        if (currentPanel != null)
        {
            var screenIdVariable = currentPanel.GetVariable("ScreenId");
            if (screenIdVariable != null)
                pageChangeTag.Value = screenIdVariable.Value;
        }
    }

    private void RecursiveSearch(IUANode inputObject)
    {
        foreach (var childrenObject in inputObject.Children)
        {
            try
            {
                if (childrenObject is FTOptix.Core.Folder)
                {
                    Log.Verbose1("FindPages.Folder", "Found folder with name [" + childrenObject.BrowseName + "] and Type: [" + childrenObject.GetType().ToString() + "]");
                    RecursiveSearch(childrenObject);
                }
                else
                {
                    var screenIdVariable = ((UAManagedCore.UAObjectType)childrenObject).GetVariable("ScreenId");
                    if (screenIdVariable == null)
                    {
                        Log.Verbose1("FindPages.Variable", "Found object with name [" + childrenObject.BrowseName + "] and Type: [" + childrenObject.GetType().ToString() + "] but it does not have a ScreenId variable");
                        continue;
                    }

                    int pageId = screenIdVariable.Value;
                    if (pageId > 0)
                    {
                        Log.Debug("FindPages", "Found page with name [" + childrenObject.BrowseName + "] and ID: [" + pageId.ToString() + "]");
                        if (pagesDictionary.TryGetValue((uint)pageId, out var destinationPage))
                            Log.Warning("FindPages.Page", "Found page with name [" + childrenObject.BrowseName + "] and duplicate ID: [" + pageId.ToString() + "]");
                        else
                            pagesDictionary.Add((uint)pageId, childrenObject.NodeId);
                    }
                    else
                    {
                        Log.Warning("FindPages.Page", "Found page with name [" + childrenObject.BrowseName + "] and invalid ID: [" + pageId.ToString() + "]");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error("FindPages.Catch", "Exception thrown: " + ex.Message);
            }
        }
    }

    private PanelLoader panelLoader;
    private IUAVariable pageChangeTag;
    private readonly Dictionary<uint, NodeId> pagesDictionary = new Dictionary<uint, NodeId>();
    private LongRunningTask myLongRunningTask;
}
