﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Reflection;
using System.Collections;

namespace Helpers
{

    public class MenuHelper
    {
        protected List<MenuItemBase> Options { get; }
        public MenuSettings Settings { get; }

        public MenuFlags SettingsFlags
        {
            get
            {
                return MenuFlags.ClearScreenFirst;
            }
            set
            {
                Settings.ClearScreenFirst = value.HasFlag(MenuFlags.ClearScreenFirst);
                Settings.Flags = value;

            }
        }

        public MenuHelper()
        {
            Settings = new MenuSettings();
            Options = new List<MenuItemBase>();
        }

        public void Clear()
        {
            Options.Clear();
        }

        public void Build(Func<List<MenuItemBase>> optionGenerator, Action<Type> itemRunner = null)
        {
            var options = optionGenerator();
            for (var o = 0; o < options.Count; o++)
            {
                var opt = options[o];
                opt.Run = itemRunner;
                Options.Add(opt);
            }
        }

        public MenuItemBase this[int i]
        {
            get => Options.SingleOrDefault(o => o.Index == i);
        }

        public static List<MenuItemBase> GetTypedMenuItems(List<Type> types)
        {
            return types.Select(t => new MenuItemBase() { Index = types.IndexOf(t) + 1, Text = t.FullName, ImplementedAs = t }).ToList();
        }

        public void DisplayMenu()
        {
            if (Settings.ClearScreenFirst)
            {
                Console.Clear();
            }

            foreach(var option in Options)
            {
                Console.WriteLine($"{option.Index,-3} - {option.Text}" +
                    $"{(Settings.DefaultOption == option.Index ? " (default)" : "")}");
            }

            if (Settings.GenerateExitOption)
            {
                if (Settings.SelectWithReadKey)
                {
                    Console.WriteLine($"{Settings.ExitKey} - Exit");
                }
                else
                {
                    Settings.ExitOption = Options.Count + 1;
                    Console.WriteLine($"{Settings.ExitOption,-3} - Exit");
                }
            }
            Console.WriteLine();
        }

        public int SelectFromMenu(string prompt = null)
        {
            if (prompt == null)
                prompt = Settings.Prompt;

            DisplayMenu();

            var selection = 0;

            var gotSelection = (Settings.Flags.HasFlag(MenuFlags.SelectWithReadKey))
                ? ConsoleHelper.GetInteger(prompt, out selection, new KeyRangeValidator(1, Options.Count))
                : ConsoleHelper.GetInteger(prompt, out selection, new RangeValidator(1, Options.Count));

            if (gotSelection)
            {
                var opt = this[selection];
                if (opt.Run != null)
                    opt.Run(opt.ImplementedAs);
            }
            return gotSelection ? selection : Settings.DefaultOption;
        }

        public int AddOption(string optionText)
        {
            var newOpt = new MenuItemBase() { Index = Options.Count + 1, Text = optionText };
            Options.Add(newOpt);
            return newOpt.Index;
        }

        public void Configure(MenuFlags flags, string prompt, int defaultOption)
        {
            Configure(flags.HasFlag(MenuFlags.SelectWithReadKey), ConsoleKey.Escape, defaultOption, flags.HasFlag(MenuFlags.GenerateExitOption)
                , prompt, flags.HasFlag(MenuFlags.ClearScreenFirst), flags);
        }


        public static MenuSelectionItem SelectByKeySequence(List<KeyStep<int>> steps)
        {
            var cw = typeof(Console).GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.ExactBinding)
                .Where(m => m.Name == "Write" && m.GetParameters().Length == 2 && m.GetParameters()[1].ParameterType == typeof(object[])).FirstOrDefault();

            foreach (var step in steps)
            {
                var cwParams = steps.Select(s => $"{s.Response}").ToArray();
                Console.Write(step.Prompt, cwParams);
               
                step.Key = Console.ReadKey().Key;
                Console.WriteLine();
                step.Response = step.Translate(step.Key);
            }
            return null;
        }

        public static MenuSelection SelectMulti(IMenu menu, List<KeyStep<int>> steps)
        {
            Console.Clear();
            menu.Display();

            var selection = new MenuSelection();
            var ynKey = ConsoleKey.N;
            do
            {
                var key = default(ConsoleKey);
                do
                {
                    var selItem = SelectByKeySequence(steps);
                    Console.WriteLine("\n");
                    key = Console.ReadKey().Key;
                }
                while (key != ConsoleKey.Enter);

                ynKey = ConsoleHelper.GetKey("Enter Y to confirm your selection, N to re-enter your selection or Q to quit: ", ConsoleKey.Y, ConsoleKey.N);
            }
            while (ynKey == ConsoleKey.N);

            if (ynKey == ConsoleKey.Q)
            {
                selection = null;
            }    
            return selection;
        }

        public void Configure(MenuFlags flags, string prompt, int defaultOption, ConsoleKey exitKey)
        {
            Configure(flags.HasFlag(MenuFlags.SelectWithReadKey), exitKey, defaultOption, flags.HasFlag(MenuFlags.GenerateExitOption)
                , prompt, flags.HasFlag(MenuFlags.ClearScreenFirst), flags);
        }
        public void Configure(bool selectWithReadKey, ConsoleKey exitKey, int defaultOption, 
            bool generateExitOption, string prompt, bool clearScreenFirst, MenuFlags flags)
        {
            Settings.ClearScreenFirst = clearScreenFirst;
            Settings.DefaultOption = defaultOption;
            Settings.ExitKey = exitKey;
            Settings.Flags = flags;
            Settings.GenerateExitOption = generateExitOption;
            Settings.Prompt = prompt;
            Settings.SelectWithReadKey = selectWithReadKey;
        }

        public void RealiseSelectedOption()
        {
            /* sort this out
            var ctors = options[selection - 1].GetConstructors();
               if (ctors.Any(c => c.GetParameters().Length == 0))
               {
                   return (T) options[selection - 1].Assembly.CreateInstance(options[selection - 1].FullName);
               }
            */
            var ctors = typeof(MenuItemBase).GetConstructors();
            var minArgs = ctors.Min(m => m.GetParameters().Length);
            var minCtor = ctors.Single(c => c.GetParameters().Length == minArgs);
            var args = minCtor.GetParameters();
            var instArgs = new List<object>();
            for (var i = 0; i < args.Length; i++)
            {
                var arg = args[i];
                if (arg.ParameterType.IsArray)
                {
                    var children = arg.ParameterType.Assembly.DefinedTypes
                        .Where(t => t.BaseType?.FullName == arg.ParameterType.FullName.Replace("[]", ""))
                        .ToList();
                    foreach (var c in children)
                    {
                        instArgs.Add(c.Assembly.CreateInstance(c.FullName));
                    }
                    continue;
                }
                instArgs.Add(arg.ParameterType.Assembly.CreateInstance(arg.ParameterType.FullName));
            }
            /*...and this
                        return (T) options[selection - 1].Assembly.CreateInstance(
                            options[selection - 1].FullName, false, System.Reflection.BindingFlags.Default, null, instArgs.ToArray(), null, null);
                    }
            */
        }

        public IEnumerator GetEnumerator()
        {
            return ((IEnumerable)Options).GetEnumerator();
        }
    }
}
