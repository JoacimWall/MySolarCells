namespace MySolarCells.Controls;

public partial class TemplatedContentPresenter : ContentView
{
    public TemplatedContentPresenter()
    {
        InitializeComponent();
    }

    public static readonly BindableProperty DataTemplateSelectorProperty = BindableProperty.Create(nameof(DataTemplateSelector), typeof(DataTemplateSelector), typeof(TemplatedContentPresenter), propertyChanged: DataTemplateSelectorChanged);
    public static readonly BindableProperty DataTemplateProperty = BindableProperty.Create(nameof(DataTemplate), typeof(DataTemplate), typeof(TemplatedContentPresenter), propertyChanged: DataTemplateChanged);
    public static readonly BindableProperty DataProperty = BindableProperty.Create(nameof(Data), typeof(object), typeof(TemplatedContentPresenter),  propertyChanged: DataChanged);

    public DataTemplateSelector DataTemplateSelector
    {
        get => (DataTemplateSelector)GetValue(DataTemplateSelectorProperty);
        set => SetValue(DataTemplateSelectorProperty, value);
    }

    public DataTemplate DataTemplate
    {
        get => (DataTemplate)GetValue(DataTemplateProperty);
        set => SetValue(DataTemplateProperty, value);
    }

    public object Data
    {
        get => GetValue(DataProperty);
        set => SetValue(DataProperty, value);
    }

    private static void DataTemplateSelectorChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is TemplatedContentPresenter contentPresenter && newValue is DataTemplateSelector dataTemplateSelector)
        {
            BindableLayout.SetItemTemplateSelector(contentPresenter.HostGrid, dataTemplateSelector);
        }
    }
    private static void DataTemplateChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is TemplatedContentPresenter contentPresenter && newValue is DataTemplate dataTemplate)
        {
            BindableLayout.SetItemTemplate(contentPresenter.HostGrid, dataTemplate);
        }
    }

    private static void DataChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is TemplatedContentPresenter contentPresenter)
        {
            BindableLayout.SetItemsSource(contentPresenter.HostGrid, new[] { newValue });
        }
    }
}
