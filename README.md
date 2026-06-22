# Inventory System

Event-driven item, slot, and inventory framework with interactive UI support for Unity.

Built to be extended via inheritance and composition for custom item, slot, and inventory behaviors.

---

## Features

### Item

- Create custom `Item<TItemSO>` types
- Create custom `ItemSO<TItem>` definition types

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
- Runtime inventory creation
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
Inventory<Slot> inventory = new(m_slotCount, ConstructSlot);

Slot ConstructSlot(int index) => new Slot();
```

> or

```csharp
Inventory<Slot> inventory = InventoryFactory.EmptySlots<Slot>(m_slotCount);
```

---

### UI Setup

> Slot

```csharp
ISlot slot = ...;
SlotUI slotUI = ...;
slotUI.SetSlot(slot);
```

> Inventory

```csharp
IInventory inventory = ...;
InventoryUI inventoryUI = ...;
inventoryUI.SetInventory(inventory);
```
