# Настройка XR (OpenXR + XR Hands + XRI Toolkit) для проекта

Этот проект хранит только `Assets/` и `Docs/`. Папки `Packages/` и `ProjectSettings/` Unity создаст/обновит автоматически при первом открытии проекта в Unity.

## 1) Установка пакетов

Откройте Unity → **Window → Package Manager** и убедитесь, что в проекте установлены:

- **XR Plug-in Management** (`com.unity.xr.management`)
- **OpenXR Plugin** (`com.unity.xr.openxr`)
- **XR Interaction Toolkit** (`com.unity.xr.interaction.toolkit`)
- **XR Hands** (`com.unity.xr.hands`)
- **Input System** (`com.unity.inputsystem`) (рекомендуется)

Также в репозитории добавлен `Packages/manifest.json` с зависимостями: Unity подтянет их автоматически после открытия проекта.

## 2) Project Settings → XR Plug-in Management

1. **Edit → Project Settings → XR Plug-in Management**
2. Во вкладке **PC, Mac & Linux Standalone** включите **OpenXR**
3. Во вкладке **Android** включите **OpenXR** (если планируется билд на Quest)

## 3) Project Settings → OpenXR

1. **Edit → Project Settings → XR Plug-in Management → OpenXR**
2. В разделе **Interaction Profiles** добавьте/включите:
   - **Hand Interaction Profile**

## 4) XR Device Simulator

В XRI 3.x XR Device Simulator поставляется как Sample.

1. **Window → Package Manager → XR Interaction Toolkit**
2. Вкладка **Samples**
3. Импортируйте **XR Device Simulator**

## 5) Автонастройка (в проекте)

В `Assets/Editor/AutoXRProjectSetup.cs` есть best-effort автонастройка:

- Пытается включить OpenXR как Loader для Standalone/Android
- Пытается включить **Hand Interaction Profile**

Если пакеты ещё не установлены, скрипт просто выведет сообщение в Console — после установки пакетов и перезапуска Unity он попробует снова.

