using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalPracticeLab_2_CopyGroup
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class CopyGroup : IExternalCommand

    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            Document doc = uiDoc.Document;

            Reference reference = uiDoc.Selection.PickObject(ObjectType.Element, "Выберите группу объектов");//если мы хотим изменить или скопировать объект, то ссылки не достаточно. Необходимо получить сам объект следующим методом:
            Element element = doc.GetElement(reference);//Element - родительский класс всех элементов проекта в Revit

            Group group = element as Group;//или (Group)element. Но это плохой способ, т.к. вызовет исключение в случае выбора пользователем не группы. Преобразование с помощью as не выдаст исключение, а присвоит переменной значение null.

            XYZ point = uiDoc.Selection.PickPoint("Выберите точку");

            Transaction transaction = new Transaction(doc);
            transaction.Start("Копирование группы объектов");

            doc.Create.PlaceGroup(point, group.GroupType);

            transaction.Commit();

            return Result.Succeeded;
        }
    }
}
