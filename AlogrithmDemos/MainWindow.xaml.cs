using AlogrithmDemos.Models;
using AlogrithmDemos.Models.Sorting;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AlogrithmDemos
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            List<Category> modelCategories = new List<Category>();

            Category combinatorics = new Category() { Name = "Combinatorics" };
            combinatorics.Models.Add(new SteppableModelWrapper(new TriominosModel(2, 6)));
            modelCategories.Add(combinatorics);

            Category sorting = new Category() { Name = "Sorting" };
            sorting.Models.Add(new SteppableModelWrapper(new BubbleSort(128)));
            sorting.Models.Add(new SteppableModelWrapper(new OptimisedBubbleSort(128)));
            sorting.Models.Add(new SteppableModelWrapper(new CocktailSort(128)));
            sorting.Models.Add(new SteppableModelWrapper(new OddEvenSort(128)));
            sorting.Models.Add(new SteppableModelWrapper(new InsertionSort(128)));
            sorting.Models.Add(new SteppableModelWrapper(new QuickSort(128)));
            sorting.Models.Add(new SteppableModelWrapper(new HeapSort(128)));
            sorting.Models.Add(new SteppableModelWrapper(new MergeSort(128)));
            sorting.Models.Add(new SteppableModelWrapper(new RadixLSDSort(128)));
            sorting.Models.Add(new SteppableModelWrapper(new RadixMSDSort(128)));
            modelCategories.Add(sorting);

            treeview_Models.ItemsSource = modelCategories;
        }
    }

    public class Category
    {
        public string Name { get; set; } = "Unknown";

        public ObservableCollection<SteppableModelWrapper> Models { get; set; }

        public Category()
        {
            Models = new ObservableCollection<SteppableModelWrapper>();
        }
    }

    public class SteppableModelWrapper
    {
        public ISteppableModel Model { get; set; } = null;

        public SteppableModelWrapper()
        {

        }

        public SteppableModelWrapper(ISteppableModel model)
        {
            Model = model;
        }
    }

    public class SteppableModelDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate CategoryTemplate { get; set; }
        public DataTemplate TriominoDataTemplate { get; set; }
        public DataTemplate SortAlgorithmDataTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is SteppableModelWrapper)
            {
                SteppableModelWrapper wrapper = item as SteppableModelWrapper;

                switch (wrapper.Model)
                {
                    case TriominosModel triominosModel:
                        return TriominoDataTemplate;
                    case SortAlgorithm sortAlgorithm:
                        return SortAlgorithmDataTemplate;
                }
            }
            else if (item is Category)
            {
                return CategoryTemplate;
            }

            return base.SelectTemplate(item, container);
        }
    }
}
