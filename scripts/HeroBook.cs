using Godot;
using System.Collections.Generic;

public partial class HeroBook : Panel
{
    [Export] public TextureButton BackButton;
    [Export] public TextureButton NextButton;
    [Export] public TextureButton PreviousButton;

    [Export] public Control NecromancerPage;
    [Export] public Control EnchantressPage;
    [Export] public Control ClericPage;
    
    private Dictionary<string, Control> pages = new Dictionary<string, Control>();
    private Dictionary<int, string> pageLookup = new Dictionary<int, string>();
    private Dictionary<string, int> indexLookup = new Dictionary<string, int>();

    private Control currentPage = null;
    private int pageIndex = 0;
    
    public override void _Ready()
    {
        BackButton.Pressed += () =>
        {
            Main.Instance.GoToMainPanel();
            SoundManager.Instance.PlaySound("back_pressed");
        };
        NextButton.Pressed += GoToNextPage;
        PreviousButton.Pressed += GoToPreviousPage;

        pages.Add("Necromancer", NecromancerPage);
        pages.Add("Enchantress", EnchantressPage);
        pages.Add("Cleric", ClericPage);

        int index = 0;
        foreach (var page in pages)
        {
            pageLookup.Add(index, page.Key);
            indexLookup.Add(page.Key, index);
            index++;
        }
        
        currentPage = pages["Necromancer"];
    }

    public void GoToNextPage()
    {
        currentPage.Hide();
        pageIndex++;
        pageIndex %= pages.Count;
        currentPage = pages[pageLookup[pageIndex]];
        currentPage.Show();
        SoundManager.Instance.PlaySound("poster_pressed");
    }

    public void GoToPreviousPage()
    {
        currentPage.Hide();
        pageIndex--;
        if (pageIndex < 0)
        {
            pageIndex = pages.Count - 1;
        }
        currentPage = pages[pageLookup[pageIndex]];
        currentPage.Show();
        SoundManager.Instance.PlaySound("poster_pressed");
    }

    public void ClearAllPages()
    {
        foreach (var page in pages)
        {
            page.Value.Hide();
        }
    }

    public void GoToPage(string pageName)
    {
        ClearAllPages();
        currentPage = pages[pageName];
        pageIndex = indexLookup[pageName];
        currentPage.Show();
    }
}