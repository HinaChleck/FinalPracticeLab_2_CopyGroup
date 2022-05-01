using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
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
            try
            {

            
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            Document doc = uiDoc.Document;

            GroupPickFilter groupPickFilter = new GroupPickFilter();// создаем экземпляр созданного нами selectionFilter, для применения его в перегрузке метода PickObject

            Reference reference = uiDoc.Selection.PickObject(ObjectType.Element, groupPickFilter ,"Выберите группу объектов");//если мы хотим изменить или скопировать объект, то ссылки не достаточно. Необходимо получить сам объект следующим методом:
            Element element = doc.GetElement(reference);//Element - родительский класс всех элементов проекта в Revit

            Group group = element as Group;//или (Group)element. Но это плохой способ, т.к. вызовет исключение в случае выбора пользователем не группы. Преобразование с помощью as не выдаст исключение, а присвоит переменной значение null.
            
            XYZ groupCenter = GetElementCenter(group);
            Room room = GetRoomByPoint(doc, groupCenter);
            XYZ roomCenter = GetElementCenter(room);
            XYZ offset = groupCenter - roomCenter;//определяем смещения центра группы относительно исходной комнаты

            XYZ point = uiDoc.Selection.PickPoint("Выберите точку");

            Room roomToInsert = GetRoomByPoint(doc, point);
            XYZ roomToInsertCenter = GetElementCenter(roomToInsert);
            XYZ insertPoint = roomToInsertCenter + offset;//определяем точку вставки в комнате с учетом смещения центра группы относительно центра комнаты

            Transaction transaction = new Transaction(doc);
            transaction.Start("Копирование группы объектов");

            doc.Create.PlaceGroup(insertPoint, group.GroupType);

            transaction.Commit();
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                return Result.Cancelled; //если пользователь нажмет esc
            }
            catch (Exception ex)
            {
                message = ex.Message;//для всех остальных исключений
                return Result.Failed;
            }

            return Result.Succeeded;
        }

        public XYZ GetElementCenter(Element element)      //самый легкий, но не самый точный способ. Считаем, что центр BoundingBox и центр группы совпадают
        {
            BoundingBoxXYZ bounding = element.get_BoundingBox(null);
            return (bounding.Max+bounding.Min)/2;
        }

        public Room GetRoomByPoint(Document doc, XYZ point) //метод определяет в какой комнате находится точка
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            collector.OfCategory(BuiltInCategory.OST_Rooms);

            foreach (Element e  in collector)
            {
                Room room = e as Room;
                if (room != null)
                {
                    if (room.IsPointInRoom(point))
                    {
                        return room;
                    }
                }
              }
            return null;
        }
    }

    public class GroupPickFilter : ISelectionFilter //создаем selectionfIlter для фильтрации выбора объектов
    {
        public bool AllowElement(Element elem)
        {
            if (elem.Category.Id.IntegerValue == (int)BuiltInCategory.OST_IOSModelGroups)
                return true;//доступны к выбору будут только элементы определенной категории (сравниваем по числовому значению int)
            else
                return false;
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return false;//ссылки на элементы в принципе будут не доступны к выбору
        }
    }
}
