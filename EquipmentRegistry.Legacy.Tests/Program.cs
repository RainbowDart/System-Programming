using EquipmentRegistry.Legacy;

var repository = new EquipmentRepository();
var service = new InventoryService(repository);
var failures = new List<string>();

void Check(string name, bool condition)
{
    Console.WriteLine($"{(condition ? "PASS" : "FAIL")} — {name}");
    if (!condition) failures.Add(name);
}
/*
 * В InventoryService.cs метод GetByStatus — там написано != вместо ==.
 * Из-за этого, когда ты просишь показать "В ремонте", он показывает всё, что не в ремонте.

В SearchByName — поиск чувствителен к регистру. Если ввести "laptop", он найдет Laptop Dell , а если ввести LAPTOP  уже не найдет.
а если ввести пустую строку, он выведет вообще всё

В CalculateBookValue - амортизация считается так, что если оборудование старое его стоимость уходит в минус.
а комментариях написано, что стоимость не может быть < 0
 */
// Эти проверки — отправная точка для расследования. Некоторые намеренно выявляют дефекты.
Check("поиск без учёта регистра находит Laptop Dell",
    service.SearchByName("laptop").Any(x => x.Id == 101));

Check("фильтр InRepair возвращает только записи InRepair",
    service.GetByStatus(EquipmentStatus.InRepair).All(x => x.Status == EquipmentStatus.InRepair));

var oldEquipment = new Equipment
{
    Id = 999, Name = "Old test device", PurchasePrice = 100m,
    PurchaseDate = new DateOnly(2010, 1, 1), Status = EquipmentStatus.InUse
};
var value = service.CalculateBookValue(oldEquipment, new DateOnly(2026, 10, 27));
Check("балансовая стоимость не отрицательна", value >= 0m);

Console.WriteLine();
Console.WriteLine($"Итого: {failures.Count} проверок не пройдено из 3.");
Console.WriteLine("Ожидаемо: сначала разберитесь, какие проверки падают и почему.");
