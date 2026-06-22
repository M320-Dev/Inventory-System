# Inventory System

Event-driven item, slot, and inventory framework with interactive UI support for Unity.

---

## Features

### Item

- Create custom item types
- Create custom itemSO types

---

### Slot

- Create custom slot types
- Item containment and validation rules
- Item stack support
- Event-driven slot updates
- Event-driven UI display
- Drag-and-drop support

---

### Inventory

- Create custom inventory types
- Event-driven inventory updates
- Custom slot construction
- UI display

---

## Quick Start

### Item & ItemSO Creation

```csharp
public sealed class Fruit : Item<FruitSO> { ... }
public sealed class FruitSO : ItemSO<Fruit> { ... }
```

---

### Inventory Construction

```csharp
Inventory<Slot> inventory;

inventory = new(m_slotCount, ConstructSlot);
inventory = InventoryFactory.EmptySlots<Slot>(m_slotCount);

Slot ConstructSlot(int index) => new Slot();
```

---

### UI Setup

```csharp
SlotUI slotUI = ...;
InventoryUI inventoryUI = ...;

ISlot slot = ...;
IInventory inventory = ...;

slotUI.SetSlot(slot);
inventoryUI.SetInventory(inventory);
```
