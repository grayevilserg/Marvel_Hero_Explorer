using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Marver_hero_explorer.Materials;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Media.Imaging;
using System.Threading.Tasks;

// Документацию по шаблону элемента "Пустая страница" см. по адресу https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x419

namespace Marver_hero_explorer
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public ObservableCollection<Character> MarvelCharacters{ get; set;}

        public MainPage()
        {
            this.InitializeComponent();

            MarvelCharacters = new ObservableCollection<Character>();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            MyRing.IsActive = true;
            MyRing.Visibility = Visibility.Visible;

            while (MarvelCharacters.Count < 10)
            {
                Task t = Facade.PopulateMarvelCharactersAsync(MarvelCharacters);
                await t;
            }
            

            MyRing.IsActive = false;
            MyRing.Visibility = Visibility.Collapsed;
        }

        private void ItemClick(object sender, ItemClickEventArgs e)
        {
            var SelectedCharacter = (Character)e.ClickedItem;

            NameTextBlock.Text = SelectedCharacter.name;
            DescriptionTextBlock.Text = SelectedCharacter.description;

            var LargeImage = new BitmapImage();
            Uri uri = new Uri(SelectedCharacter.thumbnail.large, UriKind.Absolute);
            LargeImage.UriSource = uri;
            DetailImage.Source = LargeImage;
        }
    }
}
