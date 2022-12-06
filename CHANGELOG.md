# Change log

## [1.7.0] – 2022-12-06

### Features

- `NativeStackedLists<T>` added. It can be used as an alternative for nested collections.

## [1.6.0] – 2022-09-08

### Features

- `(ReadOnly)Span` extensions for `Unity.Collections`.

## [1.5.0] – 2022-08-08

### Features

- `NativePointQuadtree` added. Implemented quadtree supports for AABB range query.
- `Reinterpret<T>` and `ElementAt` methods for `NativeIndexedArray<T>` added.
- `Ref<T>` class added. It is handy for using with native collections.

### Changes

- A small bounding volume structs `AABB` and `MBC` refactor.
- A few code style fixes.

## [1.4.0] – 2022-04-23

### Features

- `NativeArray2d<T>` added. Wrapper for two-dimensional `NativeArray<T>`. Support for reading rows in parallel jobs.
- `Native2dTree` added. Currently, only AABB Range query is implemented, NN and k-NN search are TODO.
- `NativeBoundingVolumeTree<AABB>` tree vs. tree intersections implementation using queue. The result is stored in `NativeList<int2>`.

### Changes

- Renaming `BoundingVolumeTree<T>` into `NativeBoundingVolumeTree<T>`. Old class is still available, but is marked with Obsolete attribute. Additionally fix some minor typoes in methods names and adds `ReadOnly` substruct.

## [1.3.0] – 2022-01-24

### Features

- `Span<T>` and `ReadOnlySpan<T>` support for Native Indexed Collections added.
- Debug View for `NativeIndexedArray`, `NativeIndexedList`, and `NativeStack` added.

## [1.2.0] – 2021-12-09

### Features

- `IdEnumerator<Id>` and `IdValueEnumerator<Id, T>` added.
- Unity version update: 2021.2.5f1.

## [1.1.0] – 2021-12-04

### Features

- `Id<T>` added.
- `NativeIndexedArray<Id, T>` added.
- `NativeIndexedList<Id, T>` added.
- `NativeAccumulatedProduct<T, Op>` added.
- `IAbelianOperator` and basic implementations added.

## [1.0.0] ⁠– 2021-11-28

### Features

- `NativeStack<T>` added.
- `BoundingVolumeTree<T>` added.
