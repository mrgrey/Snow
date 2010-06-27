#region Copyright&License
//   Snow - BasePC Emulator
//   Copyright (C) 2008 cleancode.ru
//
//   This program is free software: you can redistribute it and/or modify
//   it under the terms of the GNU General Public License as published by
//   the Free Software Foundation, either version 3 of the License,
//   or any later version.
//
//   This program is distributed in the hope that it will be useful,
//   but WITHOUT ANY WARRANTY; without even the implied warranty of
//   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//   GNU General Public License for more details.
//
//   You should have received a copy of the GNU General Public License
//   along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
//   Authors: Denis Bykov (mailto:thorn@cleancode.ru), Chuyko Yuriy (mr_grey@cleancode.ru)
#endregion
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Reflection;
using System.ComponentModel;


namespace Cleancode.Snow
{
    /// <summary>
    /// Класс реализующий выбор действия
    /// </summary>
    /// <typeparam name="ActionReturnType">Формат возвращаемого результата действия</typeparam>
    class ActionSelector<ActionReturnType>
    {
        //Список допустимых типов параметров
        List<Type> _acceptableTypes;
        Dictionary<Type, TypeConverter> _acceptableTypesConverters;

        // Корень дерева действий
        ActionsTreeNode _searchTreeRoot;

        public ActionSelector()
        {
            _searchTreeRoot = new ActionsTreeNode();
            _acceptableTypes = new List<Type>();
            _acceptableTypesConverters = new Dictionary<Type, TypeConverter>();
        }

        public ActionSelector(IEnumerable<Type> types)
            : this()
        {
            RegisterAcceptableTypes(types);
        }

        public bool RegisterAcceptableType(Type type)
        {
            _acceptableTypes.Add(type);
            _acceptableTypesConverters.Add(type, TypeDescriptor.GetConverter(type));

            return true;
        }

        public bool RegisterAcceptableTypes(IEnumerable<Type> types)
        {
            foreach (Type currentType in types)
                RegisterAcceptableType(currentType);
            return true;
        }

        /// <summary>
        /// Зарегистрировать набор действий
        /// </summary>
        /// <param name="actionsContainerType">Тип action container'а</param>
        /// <param name="actionsContainerInstance">Экземпляр action container'а</param>
        /// <param name="quiet">Определяет поведение в случае если метод выдающий себя за действие не подходит по какому либо параметру</param>
        public void _registerActionsFromContainer(Type actionsContainerType, object actionsContainerInstance, bool quiet)
        {
            //Проверяем, что переданный нам action container является классом и имеет набор custom'ных атттрибутов
            if (!actionsContainerType.IsClass || actionsContainerType.GetCustomAttributes(typeof(ActionsContainerAttribute), true).Length == 0)
                //TODO QUIET
                throw new Exception("unsupported actions container");
            BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Static;

            //Если наш action container не статический, то включаем в поиск instance методы
            if (actionsContainerInstance != null)
                bindingFlags |= BindingFlags.Instance;

            //Получаем все методы контейнера удовлетворяющие указанным условиям
            MethodInfo[] maybeOurMethods = actionsContainerType.GetMethods(bindingFlags);
            ActionAttribute[] currentMethodAttributes;
            //Выбираем подходящие нам методы и заполняем ими узел дерева
            foreach (MethodInfo currentMethod in maybeOurMethods)
            {
                currentMethodAttributes = (currentMethod.GetCustomAttributes(typeof(ActionAttribute), true) as ActionAttribute[]);
                //одна из проверок все таки лишняя!!!
                if (currentMethodAttributes == null || currentMethodAttributes.Length == 0)
                    continue;
                //Если тип возвращаемого значения не совпадает с указанынм при инициализации системы.. смело мрем
                if (currentMethod.ReturnType != typeof(ActionReturnType))
                    if (!quiet)
                        throw new UnsupportedActionReturnTypeException(currentMethod);
                    else
                        continue;

                foreach (ParameterInfo currentParam in currentMethod.GetParameters())
                    if (!_acceptableTypes.Contains(currentParam.ParameterType))
                        if (!quiet)
                            throw new UnsupportedActionParamException(currentMethod, currentParam);
                        else
                            continue;

                string[] commandPath;
                foreach (ActionAttribute currentAttribute in currentMethodAttributes)
                {
                    commandPath = currentAttribute.CmdParts;
                    ActionsTreeNode currentNode = _searchTreeRoot;
                    for (int e = 0; e < commandPath.Length; e++)
                    {
                        if (!currentNode.Childs.ContainsKey(commandPath[e]))
                            currentNode.Childs.Add(commandPath[e], new ActionsTreeNode());
                        currentNode = currentNode.Childs[commandPath[e]];
                    }
                    currentNode.Actions.Add(new ActionInfo(actionsContainerInstance, currentMethod));
                }
            }
        }

        /// <summary>
        /// Зарегистрировать набор действий
        /// </summary>
        /// <param name="actionsContainerInstance">Экземпляр action container'а</param>
        /// <param name="quiet">Определяет поведение в случае если метод выдающий себя за действие не подходит по какому либо параметру</param>
        public void RegisterActionsFromContainer(object actionsContainerInstance, bool quiet)
        {
            _registerActionsFromContainer(actionsContainerInstance.GetType(), actionsContainerInstance, quiet);
        }

        /// <summary>
        /// Зарегистрировать набор действий из статического action container'а
        /// Т.к. action container статический, нам не нужно передавать объект, достаточно передать его тип
        /// </summary>
        /// <param name="actionsContainerType">Тип action container'а</param>
        /// <param name="quiet">Определяет поведение в случае если метод выдающий себя за действие не подходит по какому либо параметру</param>
        public void RegisterActionsFromContainer(Type actionsContainerType, bool quiet)
        {
            _registerActionsFromContainer(actionsContainerType, null, quiet);
        }

        /// <summary>
        /// Выполнить указанное действие
        /// </summary>
        /// <param name="command">Действие, которое требуется выполнить</param>
        /// <returns>Результат действия в указанном формате</returns>
        public ActionReturnType ExecuteCommand(string command)
        {
            //разделяем переданную строку(команду с аргументами) на компоненты по пробелу

            string[] commandParts = command.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            //поиск по дереву команд будет вестиcь, естественно, начиная с его корня
            ActionsTreeNode currentNode = _searchTreeRoot;
            //количество параметров\аргументов команды
            int paramsCount = commandParts.Length;

            //Ищем необходимые действия в дереве
            string knownCommand = "";
            foreach (string currentPart in commandParts)
            {
                if (!currentNode.Childs.ContainsKey(currentPart))
                    break;
                currentNode = currentNode.Childs[currentPart];
                knownCommand += currentPart + " ";
                paramsCount--;
            }

            if (currentNode.Actions.Count == 0)
                throw new Exception(String.Format("Unknown command:\n\t{0}", command));

            //Составляем список подходящих действий
            List<ActionInfo> maybeOurMethods = currentNode.Actions.FindAll(delegate(ActionInfo current)
            { return current.MethodParams.Count == paramsCount; });

            if (maybeOurMethods.Count == 0)
                throw new Exception(String.Format("Wrong params count for command:\n\t{0}\nor unknown command:\n\t{1}", knownCommand, command));

            int argsStartIndex = commandParts.Length - paramsCount;
            object[] paramsToInvokeAction;
            List<Type> currentActionParamsTypes;

            bool error;
            TypeConverter currentConverter;
            foreach (ActionInfo currentAction in maybeOurMethods)
            {
                paramsToInvokeAction = new object[currentAction.MethodParams.Count];
                currentActionParamsTypes = new List<Type>(currentAction.MethodParams.Values);
                error = false;
                // Проверим подходят ли переданные параметры к найденным действиям
                for (int e = argsStartIndex; e < commandParts.Length; e++)
                {
                    currentConverter = _acceptableTypesConverters[currentActionParamsTypes[e - argsStartIndex]];

                    if (!currentConverter.CanConvertFrom(typeof(string)))
                    {
                        error = true;
                        break;
                    }
                    try
                    {
                        paramsToInvokeAction[e - argsStartIndex] = currentConverter.ConvertFrom(commandParts[e]);
                    }
                    catch
                    {
                        error = true;
                        break;
                    }
                }
                if (!error)
                {
                    try
                    {
                        //Вызвать необходимое действие
                        return (ActionReturnType)currentAction.ActionMethod.Invoke(currentAction.Instance, paramsToInvokeAction);
                    }
                    // Если внутри действия произошло исключение, то передадим его во вне
                    catch (TargetInvocationException exceptionPassedByAction)
                    {
                        throw exceptionPassedByAction.InnerException;
                    }
                    catch (Exception exception)
                    {
                        //пока просто передаем базовое исключение содержащее текст исходного
                        throw new Exception(String.Format("Error calling the action: {0}", exception.Message));
                    }
                }
            }

            throw new Exception(String.Format("Wrong params types for command:\n\t{0}\nor unknown command:\n\t{1}", knownCommand, command));
        }
    }
}
