import React, {useState, useEffect} from 'react';
import { makeStyles } from '@material-ui/core/styles';
import Table from "@material-ui/core/Table"; // Material ui table for usual form
import TableHead from "@material-ui/core/TableHead";
import TableRow from "@material-ui/core/TableRow";
import TableBody from "@material-ui/core/TableBody";
import TableCell from "@material-ui/core/TableCell";
import Button from '@material-ui/core/Button';
import { Grid } from '@material-ui/core';
import Paper from '@material-ui/core/Paper';
import GridSelect from '@material-ui/core/Select';
import FormControl from '@material-ui/core/FormControl';
import IconButton from '@material-ui/core/IconButton';
import MenuItem from '@material-ui/core/MenuItem';
import LinearProgress from '@material-ui/core/LinearProgress';
import Snackbar from '@material-ui/core/Snackbar';
import NumberFormat from 'react-number-format';
import PropTypes from 'prop-types';
import Tooltip from '@material-ui/core/Tooltip';
// Icons
import FirstPageIcon from '@material-ui/icons/FirstPage';
import ArrowForwardIosIcon from '@material-ui/icons/ArrowForwardIos';
import ArrowBackIosIcon from '@material-ui/icons/ArrowBackIos';
import ArrowDropDownIcon from '@material-ui/icons/ArrowDropDown';
import ArrowDropUpIcon from '@material-ui/icons/ArrowDropUp';
import ReplayIcon from '@material-ui/icons/Replay';
// Form components
import TextField from '@material-ui/core/TextField';
import Select from 'react-select'; // https://react-select.com/home
import Checkbox from '@material-ui/core/Checkbox';

// Custom components
// import "../../components/GridForm/GridFormCSS.css"
import Buttons from "../configuration/Buttons.json";
import GridFormButtons from '../configuration/GridFormButtons.json';
import TblFormBtns from '../configuration/TableFormButtons.json'
// Libraries
import swal from 'sweetalert'; // https://sweetalert.js.org/guides/
import hotkeys from 'hotkeys-js'; // https://github.com/jaywcjlove/hotkeys
import uuid from "uuid";
//Style
import "../styles/generalStyles.css"

var fetch = require('node-fetch');
var generator = require('generate-password');

function FloatFormat(props){
  const { inputRef, onChange, ...other } = props;
  return (
    <NumberFormat
      {...other}
      getInputRef={inputRef}
      onValueChange={values => {
        onChange({
          target: {
            value: values.value,
          },
        })
      }}
      decimalSeparator={"."}
      thousandSeparator={" "}
      isNumericString
    />
  );
}
FloatFormat.propTypes = {
  inputRef: PropTypes.func.isRequired,
  onChange: PropTypes.func.isRequired,
}
function IntegerFormat(props) {
  const { inputRef, onChange, ...other } = props;
  return (
    <NumberFormat
      {...other}
      getInputRef={inputRef}
      onValueChange={values => {
        onChange({
          target: {
            value: values.value,
          },
        });
      }}
      thousandSeparator={" "}
      isNumericString
    />
  );
}
IntegerFormat.propTypes = {
  inputRef: PropTypes.func.isRequired,
  onChange: PropTypes.func.isRequired,
}
export default (props) => {  
    // This.state
  const [kfbRESTApi] = useState(props.kfbRESTApi)
  const [taskVariables] = useState(props.taskVariables);
  const [userProfile] = useState(props.userProfile)
  const [Form, setForm] = useState(null)
  const [buttons, setButtons] = useState([])
  const [gridForm, setGridForm] = useState(null)
  const [gridFormButtons, setGridFormButtons] = useState([])
  const [tblFormBtns, setTblFromBtns] = useState([])
  const [selectedDoc, setSelectedDoc] = useState(null)
  // const [docId, setDocId] = useState(props.userTask.docId)
  const [formType] = useState("edit")
  const [docList, setDocList] = useState(null)
  const [filteredDocList, setFilteredDocList] = useState(null)
  const [initialDocList, setInitialDocList] = useState(null)  
  const [enumData] = useState([])
  const [enumOptions, setEnumOptions] = useState({})
  const [gridFormEnumData] = useState([])
  const [fieldValue, setFieldValue] = useState({})
  const [selectedOptions, setSelectedOptions] = useState([])
  
  const [page, setPage] = useState(1)
  const [size, setSize] = useState(10)
  const [showSnackBar, setShowSnackBar] = useState(false)
  const [snackBarMessage, setSnackBarMessage] = useState("")
  // const [sectionColor] = useState("#B2E0C9")
  const [updateState, setUpdateState] = useState(false)
  const [task] = useState(props.task)
  const [sortedColumn, setSortedColumn] = useState("null")
  const [sortedColumnOrder, setSortedColumnOrder] = useState(1)

  // Set data from props to state of component
  useEffect(()=>{
    console.log("MAINFORM PROPS", props)
    fetchForm(taskVariables.formDefId)
    setButtons(Buttons[userProfile.role.name][taskVariables.buttons])
    console.log("BUTTONS", Buttons[userProfile.role.name][taskVariables.buttons])
    if(taskVariables.gridFormDefId !== null){
      fetchGridFrom(taskVariables.gridFormDefId)
      fetchDocList(taskVariables.docListApi)
      setGridFormButtons(GridFormButtons[userProfile.role.name][taskVariables.gridFormButtons])
      setTblFromBtns(TblFormBtns[userProfile.role.name][taskVariables.tblFormBtns])
      console.log("G BUTTONS", GridFormButtons[userProfile.role.name][taskVariables.gridFormButtons])
      console.log("Tbl BUTTONS", TblFormBtns[userProfile.role.name][taskVariables.tblFormBtns])
    }
    if(taskVariables.selectedDoc !== "null" && taskVariables.selectedDoc !== undefined && taskVariables.selectedDoc !== null){
      // let parsedSelectedDoc = JSON.parse(props.userTask.selectedDoc)
      // let fields = {}
      // let Form = props.userTask.Form
      // for(let s=0; s<Form.sections.length; s++){
      //   for(let c=0; c<Form.sections[s].contents.length; c++){
      //     let fieldName = Form.sections[s].contents[c].name
      //     fields[fieldName] = parsedSelectedDoc[fieldName]
      //   }
      // }    
      // console.log("SDOC", parsedSelectedDoc)
      // console.log("FIELDVAL", fields)
      setSelectedDoc(taskVariables.selectedDoc)
      setFieldValue(taskVariables.selectedDoc)
    }
    // if(props.userTask.enumData !== null && props.userTask.enumData !== undefined && props.userTask.enumData !== "null"){
    //   let newEnumOptions = {}
    //   for(let i=0; i<props.userTask.enumData.length; i++){
    //     let options = [{
    //       "value": "",
    //       "label": "Пусто",
    //       "name" : props.userTask.enumData[i].name
    //     }]
    //     for(let d=0; d<props.userTask.enumData[i].data.length; d++){
    //         options.push({
    //         "value": props.userTask.enumData[i].data[d].id,
    //         "label": props.userTask.enumData[i].data[d].label,
    //         "name" : props.userTask.enumData[i].name
    //       })
    //     }
    //     newEnumOptions[props.userTask.enumData[i].name] = options
    //   }
    //   setEnumOptions(newEnumOptions)
    // }
    setUpdateState(getUUID())
  },[])
  async function fetchForm(defid){
    await fetch(kfbRESTApi + "/api/Metadata/GetByDefId?defid=" + defid,
      {
        "headers": { "content-type": "application/json" }
      }
    )
    .then(response => response.json())
    .then(function(res){
      let f = JSON.parse(res.data.data)
      setForm(f)
      console.log("FORM", f)
    }) 
  }
  // docListApi: "/api/Directory/Gets"
  async function fetchDocList(docListApi){
    await fetch(kfbRESTApi + docListApi, 
      {
        "headers": { "content-type": "application/json" }
      }
    )
    .then(response => response.json())
    .then(function(res){
      console.log("DocList", res.data)
      setFilteredDocList(res.data)
      setInitialDocList(res.data)
      let sortedDocList = res.data.sort(dynamicSort("id", 1, "Int"))
      fetchBySize(0, 9, sortedDocList)      
    })
  }
  async function fetchGridFrom(defid){
    await fetch(kfbRESTApi + "/api/Metadata/GetByDefId?defid=" + defid,
      {
        "headers": { "content-type": "application/json" }
      }
    )
    .then(response => response.json())
    .then(function(res){
      let f = JSON.parse(res.data.data)
      setGridForm(f)
      console.log("GFORM", f)
    })
  }
  
  const handleCheckboxChange = (event) => {
    console.log("CHBX", event.target.name, event.target.checked)
    setFieldValue({ ...fieldValue, [event.target.name]: event.target.checked })
  }
  // Integer input handler
  const handleIntChange = (event) => {
    console.log("EVENT", event.target.name, event.target.value)
    let stringNum = event.target.value.toString().replace(/ /g,'')
    let int = parseInt(stringNum)
    setFieldValue({ ...fieldValue, [event.target.name]: int })
  }
  // Float input handler
  const handleFloatChange = (event) => {
    let stringNum = event.target.value.toString().replace(/ /g,'');
    let float = parseFloat(stringNum)
    setFieldValue({...fieldValue, [event.target.name]: float})
    console.log("FLOAT CHANGE", event.target.name, fieldValue)
  }
  // Text input handler
  function handleChange(event){
    // console.log("EVENT", event.target.name, event.target.value)
    fieldValue[event.target.name] = event.target.value
    setFieldValue(fieldValue)
    console.log("FIELDVALUE", fieldValue)
  }
  function handleSelectChange(option){
    fieldValue[option.name] = option.value
    console.log("OPT", option.name, option.value)
    var updateSelectedOptions = selectedOptions.slice()
    let noSuchOption = true
    for(let i=0; i<updateSelectedOptions.length; i++){
      if(option.name === updateSelectedOptions[i].name){
        updateSelectedOptions[i] = option
        noSuchOption = false
        setSelectedOptions(updateSelectedOptions)
        break
      }
      else {
        noSuchOption = true
      }
    }
    if(noSuchOption === true){
      updateSelectedOptions.push(option)
      setSelectedOptions(updateSelectedOptions)
    }
  }
  function handleDateTimeChange(event){
    // console.log("EVENT", event.target.name, event.target.value)
    let fullDate = props.parseDate(event.target.value)
    var hours = new Date().getHours()
    var minutes = new Date().getMinutes()
    var seconds = new Date().getSeconds()
    var convertedDate = fullDate + " " + hours + ":" + minutes + ":" + seconds + ".123456+06"
    fieldValue[event.target.name] = convertedDate
    setFieldValue(fieldValue)
    // console.log("DATE CHANGE", event.target.value)
  }
  function getContentType(key){
    for(let s=0; s<Form.sections.length; s++){
      for(let c=0; c<Form.sections[s].contents.length; c++){
        if(key === Form.sections[s].contents[c].name){
          return Form.sections[s].contents[c].type
        }
      }
    }
  }
  // filter docList by filled parameters
  function filterDocList(fetchFrom, fetchTo, Data, fields){
    var newDocList = []
    if(Object.keys(fields).length === 0){
      newDocList = Data
    }
    else{
      for(let i=0; i<Data.length; i++){
        let match = false
        for(let key in fields){
          if(fields[key] === null || fields[key] === "" || fields[key] === undefined){
            match = true
          }
          else{
            if(Data[i][key] !== null){
              let contentType = getContentType(key)
              if(contentType === "Text"){
                try{
                  let searchField = fields[key].toLowerCase()
                  let dataField = Data[i][key].toLowerCase()
                  let includeSearch = dataField.includes(searchField)
                  if(includeSearch === true){
                    match = true
                  }
                  else{
                    match = false
                    break
                  }
                }
                catch(er){
                  console.log("ERR", er)
                  match = true
                }
              }
              else if(contentType === "Int" || contentType === "Float"){
                if(fields[key] >= 0 || fields[key] < 0){
                  let searchField = fields[key].toString()
                  let dataField = Data[i][key].toString()
                  let includeSearch = dataField.includes(searchField)
                  if(includeSearch === true){
                    match = true
                  }
                  else{
                    match = false
                    break
                  }
                }
                else{match = true}
              }
              else if(contentType === "Enum"){
                
                if(fields[key] === Data[i][key]){
                  // console.log("ENUMS", key, fields[key], Data[i][key])
                  match = true
                }
                else{
                  match = false
                  break
                }
              }
              else if(contentType === "DateTime"){
                let searchField = props.parseDate(fields[key])
                let dataField = props.parseDate(Data[i][key])
                // console.log("DATES", searchField, dataField, searchField.includes(dataField))
                if(searchField === "NaN-NaN-NaN"){
                  match = true
                }
                else{
                  if(searchField.includes(dataField) === true){
                    match = true
                  }
                  else{
                    match = false
                    break
                  }
                }
              }
              else if(contentType === "Bool"){
                if(fields[key] === Data[i][key]){
                  match = true
                }
                else{
                  match = false
                  break
                }
              }
            }
            else{
              match = false
              break
            }
          }
        }
        if(match === true){
          newDocList.push(Data[i])
        }
      }
    }
    setFilteredDocList(newDocList)
    fetchBySize(fetchFrom, fetchTo, newDocList)
  }
  // get rows amount of filtered docList by size
  function fetchBySize(fetchFrom, fetchTo, Data){
    // console.log("fetchFrom", "fetchTo")
    let newDocList = []
    for(let i=fetchFrom; i<=fetchTo; i++){
      if(Data[i] !== undefined){
        newDocList.push(Data[i])
      }
    }
    // setFilteredDocList(Data)
    setDocList(newDocList)
  }
  // Pagination functions
  function KeyboardArrowFirstClick(){
    if(page !== 1){
      setPage(1)
      fetchBySize(0, size-1, filteredDocList)
    }
    else{
      setSnackBarMessage("Вы на первой странице!")
      setShowSnackBar(true)
    }
  }
  function KeyboardArrowLeftClick(page){
    if(page !== 1){
      var prevPage = page - 1
      setPage(prevPage)
      let fetchFrom = ((prevPage -1) * size) //10
      let fetchTo = (size * prevPage)-1
      fetchBySize(fetchFrom, fetchTo, filteredDocList)
    }
    else{
      setSnackBarMessage("Вы на первой странице!")
      setShowSnackBar(true)
    }
  }
  function KeyboardArrowRightClick(page){
    if(docList.length < size-1){
      // console.log("NO DATA")
      setSnackBarMessage("Больше нет данных!")
      setShowSnackBar(true)
    } 
    else{
      setPage(page + 1)
      let fetchFrom = (size * page)
      let fetchTo = ((page + 1) * size)-1
      fetchBySize(fetchFrom, fetchTo, filteredDocList)
    }    
  }
  function handleChangeRowsPerPage(event){
    setSize(event.target.value)
    setPage(1)
    fetchBySize(0, event.target.value-1, filteredDocList)
  } 
  function GoToPage(){
    let fetchFrom = (page*size-1)-size
    let fetchTo = page*size-1
    fetchBySize(fetchFrom, fetchTo, filteredDocList)
  }
  function handlePageChange(event){
    setPage(event.target.value)
  }
  function handleCloseSnackBar(){
    setShowSnackBar(false)
  }
  function getPageAmount(){
    let pagesFloat = (filteredDocList.length)/size
    let mathRoundOfPages = Math.round(pagesFloat)
    if(pagesFloat > mathRoundOfPages){
      return mathRoundOfPages + 1
    }
    else{
      return mathRoundOfPages
    }
  }
  // random UUID generator
  function getUUID() {
    const uuid = require("uuid")
    return uuid.v1()
  }
  // random numbers generator
  function keyGen(length){
    var password = generator.generate({
      length: length,
      numbers: true
    })
    return password
  }
  function swAllert(text, icon){
    return(
      swal({
        text: text,
        icon: icon,
        buttons: {ok: "Ок"},
      })
    )
  }
  // Collect data to save doc
  function getFieldValuesSaveDocument(){
    let docToSave = {}
    for(let s=0; s<Form.sections.length; s++){
      for(let c=0; c<Form.sections[s].contents.length; c++){
        let fieldName = Form.sections[s].contents[c].name
        if(Form.sections[s].contents[c].type === "Bool" && fieldValue[fieldName] === undefined){
          docToSave[fieldName] = false
        }
        else if(Form.sections[s].contents[c].type === "BoolCustom"){
          if(fieldValue[fieldName] === true){
            docToSave[fieldName] = Form.sections[s].contents[c].valueTrue
          }
          else{
            docToSave[fieldName] = Form.sections[s].contents[c].valueFalse
          }
        }
        else{
          docToSave[fieldName] = fieldValue[fieldName]
        }
      }
    }
    return docToSave
  }
  // Collect data to update doc
  function getFieldValuesUpdateDocument(){
    let docToUpdate = {}
    docToUpdate["created_at"] = selectedDoc.created_at
    // docToUpdate["id"] = parseInt(docId)
    for(let s=0; s<Form.sections.length; s++){
      for(let c=0; c<Form.sections[s].contents.length; c++){
        let fieldName = Form.sections[s].contents[c].name
        if(Form.sections[s].contents[c].type === "Bool" && (fieldValue[fieldName] === undefined || fieldValue[fieldName] === null)){
          docToUpdate[fieldName] = false
        }
        else if(Form.sections[s].contents[c].type === "BoolCustom"){
          if(fieldValue[fieldName] === true){
            docToUpdate[fieldName] = Form.sections[s].contents[c].valueTrue
          }
          else{
            docToUpdate[fieldName] = Form.sections[s].contents[c].valueFalse
          }
        }
        else{
          docToUpdate[fieldName] = fieldValue[fieldName]
        }
      }
    }
    return docToUpdate
  }
  function checkForRequieredFields(){
    let checkedSuccessfuly = null
    for(let s=0; s<Form.sections.length; s++){
      for(let c=0; c<Form.sections[s].contents.length; c++){
        let fieldName = Form.sections[s].contents[c].name
        if(Form.sections[s].contents[c].required === true){
          if(fieldValue[fieldName] === undefined || fieldValue[fieldName] === null || 
            fieldValue[fieldName] === "NaN.NaN.NaN" || fieldValue[fieldName] === ""){
            checkedSuccessfuly = false
            swAllert("Внимание заполните поле \"" +  Form.sections[s].contents[c].label + "\"", "warning")
            return checkedSuccessfuly
          }
          else{
            checkedSuccessfuly = true
          }
        }
        else{
          checkedSuccessfuly = true
        }
      }
    }
    return checkedSuccessfuly
  }
  function buttonClick(buttonName, item){
    console.log("ITEM", item)
    if(buttonName === "findDocument"){
      // console.log("findDocument")
      filterDocList(0, size-1, initialDocList, fieldValue)
    }
    else if(buttonName === "openMainRef"){
      let taskVars = {
        formDefId: item.search_form,         
        buttons: "RefSearchBtn",
        gridFormDefId: item.grid_form,
        gridFormButtons: "Btn",
        tblFormBtns: "RefTblBtns",
        docListApi: item.controller,
        creating_form: item.creating_form,
        edit_form: item.edit_form
      }
      props.setNewTask("openMainRef", taskVars)
      console.log("button openMainRef: ", taskVars)   
    }
    else if (buttonName === "backToMainRef"){
      props.setNewTask("showMainRefSearchForm", {
        formDefId: "2aeadc9c-99f6-48fc-a2b3-a47c0670b109",
        buttons: "MainRefSearchBtn",
        gridFormDefId: "5fe6c5f6-ca17-4415-9d7d-57aed52cfad1",
        gridFormButtons: "MainRefGridBtn",
        tblFormBtns: "MainRefTblBtns",
        docListApi: "/api/Directory/Gets"
      })
    }
    else if(buttonName === "openEditRef"){
        let taskVars = {
        formDefId: taskVariables.edit_form,
        buttons:"RefSearchBtn",
        gridFormDefId: null,
        gridFormButtons: null,
        tblFormBtns: null,
        docListApi: null,
        selectedDoc: item
      }
      props.setNewTask("openEditRef", taskVars)
      console.log("button openEditRef: ", taskVars)   
    }   
    else if (buttonName === "close"){
      props.setNewTask(null, {})
      console.log("button close: ")
    }
    else if(buttonName === "editDocument"){
      // console.log("ITEM", item)
      let commandJson = 
      {
        commandType: "completeTask",
        userId: userProfile.userId,
        userRole: userProfile.userRole,
        variables: {
          userAction: {value: "editDocument"}
        }
      }
      console.log("button editDocument: ", commandJson)
      // props.sendFieldValues(commandJson)
      // props.clearTabData(process_id)
    }
    else if(buttonName === "openReferenceDocument"){
      // console.log("ITEM", item)
      let commandJson = 
      {
        userId: userProfile.userId,
        userRole: userProfile.userRole,
        variables: {
          apiName: {value: item.controller},
          userAction: {value: "openReferenceDocument"},
          searchFormDefId: {value: item.search_form},
          gridFormDefId: {value: item.grid_form},
          creatingFormDefId: {value: item.creating_form},
          editFormDefId: {value: item.edit_form},
          searchDoc: {value: JSON.stringify(fieldValue)}
        }
      }
      console.log("button openReferenceDocument: ", commandJson)
      // props.sendFieldValues(commandJson)
      // props.clearTabData(process_id)
    }
    else if(buttonName === "updateDocument"){
      let updateBody = getFieldValuesUpdateDocument()
      let commandJson = 
      {
        userId: userProfile.userId,
        userRole: userProfile.userRole,
        variables: {
          userAction: {value: "updateDocument"},
          document: {value: JSON.stringify(updateBody)}
        }
      }
      console.log("updateDocument:", commandJson)
      let checkResult = checkForRequieredFields()
      if(checkResult === true){

      }
    }
    else if(buttonName === "createDocument"){
      let commandJson = 
      {
        userId: userProfile.userId,
        userRole: userProfile.userRole,
        variables: {
          userAction: {value: "createDocument"},
          searchDoc: {value: JSON.stringify(fieldValue)}
        }
      }
      console.log("createDocument:", commandJson)

    }
    else if(buttonName === "saveDocument"){
      let docToSave = getFieldValuesSaveDocument()
      let commandJson = 
      {
        userId: userProfile.userId,
        userRole: userProfile.userRole,
        variables: {
          userAction: {value: "saveDocument"},
          document: {value: JSON.stringify(docToSave)}
        }
      }
      console.log("saveDocument:", commandJson)
      let checkResult = checkForRequieredFields()
      if(checkResult === true){
        // props.sendFieldValues(commandJson)
        // props.clearTabData(process_id)
      }
    }
    else if(buttonName === "deleteDocument"){
      let docToDelete = selectedDoc.id
      let commandJson = 
      {
        userId: userProfile.userId,
        userRole: userProfile.userRole,
        variables: {
          userAction: {value: "deleteDocument"},
          document: {value: docToDelete.toString()}
        }
      }
      console.log("deleteDocument:", commandJson)
      swal({
        text: "ВНИМАНИЕ! Вы действительно удалить данную запись?",
        icon: "warning",
        buttons: {yes: "Да", cancel: "Отмена"},
      })
      .then((click) => {
        if(click === "yes"){
          // props.sendFieldValues(commandJson)
          // props.clearTabData(process_id)
        }
      })
    }
    
    else if(buttonName === "cancel"){
      const commandJson = 
      {
        userRole: userProfile.userRole,
        variables: {
          authorization: {value: "token"},
          userAction: {value: "cancel"},
        }
      }
      console.log("button cancel: ", commandJson)
      // props.sendFieldValues(commandJson)
      // props.clearTabData(process_id)
    }
    else if(buttonName === "updateDocList"){
      const commandJson = 
      {
        variables: {
          userAction: {value: "updateDocList"},
        }
      }
      console.log("button updateDocList: ", commandJson)
      // props.sendFieldValues(commandJson)
      // props.clearTabData(process_id)
    }
    else{
      console.log("UNKNOWN Button ", buttonName)
    }
  }
  // Create sections with labels and call bodyBuilder function
  // "sections": [
  function sectionBuilder(section){
    return (
      <Table size="small" key={keyGen(5)}>
        <TableHead>
          <TableRow style={{height: 30}}>
            <TableCell 
              key={keyGen(5)}
              class="sectionBuilderStyle">
              {section.label}
            </TableCell> 
          </TableRow>
        </TableHead>
        {bodyBuilder(section)}
      </Table>
    )
  }
  // Create body of each section and call contentBuilder function
  function bodyBuilder(section){
    return(
      <Table size="small">
        <TableBody>
          {section.contents.map(contentItem=> (
            contentItem.show === true &&
              <TableRow key = {keyGen(5)} style={{height: 30}}> 
                <TableCell
                  key = {keyGen(5)}
                  class="bodyBuilderStyle">
                  {contentItem.label}
                </TableCell>                           
                <TableCell
                  key = {keyGen(5)}
                  align="left"
                  // class="bodyBuilderStyle">
                  >
                  {contentBuilder(contentItem)}
                </TableCell>
              </TableRow>
          ))} 
        </TableBody>
      </Table>
    )
  }
  // Creating components by it's type
  function contentBuilder(contentItem){
    if (contentItem.type === "Text"){
      return (
        <TextField
          onKeyPress={onKeyPressText}
          onBlur = {handleChange}
          name = {contentItem.name}
          style={{width: "100%"}}
          disabled={(formType === "view" || contentItem.edit === false) ? true : false}
          defaultValue = {(fieldValue[contentItem.name]) ? fieldValue[contentItem.name]: ""}
        />
      )
    }  
    else if (contentItem.type === "Enum"){
      let selectedOption = {
        "value": "",
        "label": "Пусто",
        "name" : contentItem.name
      }
      if(fieldValue[contentItem.name] !== undefined){
        for(let i=0; i<enumOptions[contentItem.name].length; i++){
          if(parseInt(fieldValue[contentItem.name]) === enumOptions[contentItem.name][i].value){
            selectedOption = enumOptions[contentItem.name][i]
            break
          }
        }
      }
      if(selectedOptions.length !== 0){
        for(let i=0; i<selectedOptions.length; i++){
          if(contentItem.name === selectedOptions[i].name){
            selectedOption = selectedOptions[i]
          }
        }
      }
      return (
        <Select
          name = {contentItem.name}
          value = {selectedOption}
          onChange={handleSelectChange}
          options={enumOptions[contentItem.name]}
          isDisabled ={(formType === "view" || contentItem.edit === false) ? true : false}
        />
      )
    }
    else if(contentItem.type === "Bool"){
      return(
        <Checkbox
          style={{maxWidth: 20, height: 15, color: (formType === "view" || contentItem.edit === false) ? "grey" : "green"}}
          name = {contentItem.name}
          onChange={handleCheckboxChange}
          disabled={(formType === "view" || contentItem.edit === false) ? true : false}
          checked = {(fieldValue[contentItem.name] === false || fieldValue[contentItem.name] === null || fieldValue[contentItem.name] === undefined) ? false : true}
        />
      )
    }

    else if(contentItem.type === "Int"){
      return (
        <TextField
          onKeyPress={onKeyPressInt}
          disabled={(formType === "view" || contentItem.edit === false) ? true : false}
          style={{width: "100%"}}
          defaultValue = {(fieldValue[contentItem.name] !== undefined) ? fieldValue[contentItem.name]: ""}
          // value = {(fieldValue[contentItem.name] !== undefined) ? fieldValue[contentItem.name]: ""}
          onBlur = {handleIntChange}
          name = {contentItem.name}
          InputProps={{inputComponent: IntegerFormat}}
        />
      )
    }
    else if(contentItem.type === "Float"){
      // console.log("FLOAT VAL", fieldValue[contentItem.name])
      return(
        <TextField
          onKeyPress={onKeyPressFloat}
          name = {contentItem.name}
          // defaultValue = {(fieldValue[contentItem.name]) ? fieldValue[contentItem.name]: ""}
          onBlur = {handleFloatChange}
          value = {fieldValue[contentItem.name]}
          style={{width: "100%"}}
          disabled={(formType === "view" || contentItem.edit === false) ? true : false}
          InputProps={{inputComponent: FloatFormat}}
        />
      )
    }
    else if (contentItem.type === "DateTime"){
      return (
        <TextField
          onKeyPress={onKeyPressDateTime}
          type="date"
          name = {contentItem.name}
          onBlur = {handleDateTimeChange}
          style={{width: "100%"}}
          defaultValue = {(fieldValue[contentItem.name] !== undefined) ? props.parseDate(fieldValue[contentItem.name]): ""}
          disabled={(formType === "view" || contentItem.edit === false) ? true : false}
          InputLabelProps={{
            shrink: true,
          }}
        />
      )
    }
  }
  // Key press handlers for some fields
  function onKeyPressText(event){
    let code = event.charCode
    if(code === 13){
      for(let i=0; i<buttons.length; i++){
        if(buttons[i].name === "findDocument"){
          // console.log("CODE", code)
          handleChange(event)
          buttonClick("findDocument", null)
        }
      }
    }
  }
  function onKeyPressDateTime(event){
    let code = event.charCode
    if(code === 13){
      for(let i=0; i<buttons.length; i++){
        if(buttons[i].name === "findDocument"){
          // console.log("CODE", code)
          handleDateTimeChange(event)
          buttonClick("findDocument", null)
        }
      }
    }
  }
  function onKeyPressInt(event){
    let code = event.charCode
    if(code === 13){
      for(let i=0; i<buttons.length; i++){
        if(buttons[i].name === "findDocument"){
          // console.log("CODE", code)
          let stringNum = event.target.value.toString().replace(/ /g,'')
          let int = parseInt(stringNum)
          fieldValue[event.target.name] = int
          setFieldValue(fieldValue)
          filterDocList(0, size-1, initialDocList, fieldValue)
        }
      }
    }
  }
  function onKeyPressFloat(event){
    let code = event.charCode
    if(code === 13){
      for(let i=0; i<buttons.length; i++){
        if(buttons[i].name === "findDocument"){
          let stringNum = event.target.value.toString().replace(/ /g,'')
          let float = parseFloat(stringNum)
          fieldValue[event.target.name] = float
          setFieldValue(fieldValue)
          filterDocList(0, size-1, initialDocList, fieldValue)
        }
      }
    }
  }
  // Create grid form components
  function getGridFormItems(dataItem, contentItem){
    let value = dataItem[contentItem.name]
    let name = contentItem.name
    let type = contentItem.type
    if(type === "Enum"){
      if(value === null || value === "" || value === undefined){
        return(<td align="center" style={{color: "black", fontSize: 12}}>-</td>)
      }
      else{
        for(let i=0; i<gridFormEnumData.length; i++){
          if(name === gridFormEnumData[i].name){
            for(let l=0; l<gridFormEnumData[i].data.length; l++){
              if(gridFormEnumData[i].data[l].id === parseInt(value)){
                return gridFormEnumData[i].data[l].label
              }
            }
          }
        }
      }
    }
    else if(type === "Bool"){
      // console.log("ITEM", name, value, type)
      return(
        <Checkbox
          style={{maxWidth: 20, height: 15, color: value === false ? "red" : "green"}}
          checked = {(value === false || value === null || value === undefined) ? false : true}
        />
      )
    }
    else if(type === "DateTime"){
      if(value === null || value === "" || value === undefined){
        return(<td align="center" style={{color: "black", fontSize: 12}}>-</td>)
      }
      else{
        // console.log("ITEM", dataItem, value)
        let Date = value.substring(0, 10)
        let Time = value.substring(11, 16)
        return Date + " " + Time
      }
    }
    else{
      if(value === null || value === "" || value === undefined){
        return(<td align="center" style={{color: "black", fontSize: 12}}>-</td>)
      }
      else{
        return(<td>{value}</td>)
      }
    }
  }
  function checkToShowSection(section){
    let showSection = false
    for(let i=0; i<section.contents.length; i++){
      if(section.contents[i].show === true){
        showSection = true 
        break
      }
    }
    return showSection
  }
  function checkSectionOnLastChild(i){
    let lastSection = true
    if(i === gridForm.sections.length-1){ // The last section
      return true
    }
    else{
      let nextS = i+1
      // console.log("IT", i, gridForm.sections[nextS])
      for(let s=nextS; s<gridForm.sections.length; s++){
        for(let c=0; c<gridForm.sections[s].contents.length; c++){ // Check next section
          if(gridForm.sections[s].contents[c].show === true){
            lastSection = false
            break
          }
        }
      }
    }
    return lastSection
  }
  function getForm(){
    return(
      <Grid container direction="row" justify="flex-start" spacing={0}>
        <Grid item xs={8}>
          <Paper style={{width:700}}>
            <Table class="detailForm">
              <Grid container direction="row" justify="center" alignItems="center">
                {Form.sections.map(section => {
                  return sectionBuilder(section)
                })}
              </Grid>
              <Grid direction="row" align="center">
                {buttons.map((button, index) => (
                  <Button
                    class="detailFormBtn"
                    name={button.name}
                    variant="outlined"
                    onClick = {() => buttonClick(button.name, null)}                      
                  >{button.label}
                  </Button>
                )
                )}
              </Grid>
            </Table>  
          </Paper>
        </Grid>
      </Grid>
    )
  }
  function getEnumLabel(name, id){
    for(let i=0; i<enumData.length; i++){
      if(enumData[i].name === name){
        for(let d=0; d<enumData[i].data.length; d++){
          if(enumData[i].data[d].id === id){
            return enumData[i].data[d].label
          }
        }
      }
    }
  }
  function dynamicSort(property, sortOrder, type) {
    if(type === "DateTime" || type === "Bool"){
      sortOrder = sortOrder * -1
    }
    if(type === "DateTime"){
      return function(a, b){
        if(a[property] !== null && b[property] !== null){
          let dateA = new Date(a[property].substring(0, 19))
          let timeInSecA =  dateA.getTime()/1000
          // console.log("timeInSecA", timeInSecA)
          let dateB = new Date(b[property].substring(0, 19))
          let timeInSecB =  dateB.getTime()/1000
          // console.log("timeInSecB", timeInSecB)
          var result = (timeInSecA < timeInSecB) ? -1 : (timeInSecA > timeInSecB) ? 1 : 0
          return result * sortOrder
        }
        else{
          if(a[property] === null){
            return -1 * sortOrder
          }
          return 1 * sortOrder
        }
      }
    }
    else if(type === "Int" || type === "Text" || type === "Float" || type === "Bool"){
      return function(a, b){
        var result = (a[property] < b[property]) ? -1 : (a[property] > b[property]) ? 1 : 0
        return result * sortOrder
      }
    }
    else if(type === "Enum"){
      return function(a, b){
        if(a[property] === null){
          return 1 * sortOrder
        }
        else{
          let labelA = getEnumLabel(property, a[property])
          // console.log("A", property, a[property], labelA)
          let labelB = getEnumLabel(property, b[property])
          // console.log("labelB", labelB, property, b)
          var result = (labelA < labelB) ? -1 : (labelA > labelB) ? 1 : 0
          return result * sortOrder
        }
        
      }
    }
  }
  function sortDataByColumn(name, type){
    let sortOrder = 1
    if(sortedColumn === name){
      sortOrder = sortedColumnOrder * -1
    }
    setSortedColumnOrder(sortOrder)
    setSortedColumn(name)
    let sortedDocList = filteredDocList.sort(dynamicSort(name, sortOrder, type))
    setPage(1)
    let fetchFrom = 0
    let fetchTo = size-1
    setFilteredDocList(sortedDocList)
    fetchBySize(fetchFrom, fetchTo, sortedDocList)
  }
  //Отрисовка
  if(updateState !== null){
    try{
      return(
        <Grid class="mainGrid">
          {/* Create grid table with data from doclist */}
          {Form !== null && getForm()}          
          <Grid>
            { tblFormBtns !== "null" &&
              tblFormBtns.map(button => 
              <Button
                class="tblFormBtn"
                key={button.name}
                name={button.name}
                variant="outlined"
                value={button.name}
                onClick = {() => buttonClick(button.name)}
              >{button.label}
              </Button>
            )}
          </Grid>
          <Grid class="docListGrid">
          {docList !== null && gridForm !== null &&
            <Grid container direction="row" justify="flex-start" spacing={0}>
              <Grid item sm={"auto"}>
                <Paper style={{width:700}}>
                  <table style={{width:"100%", textAlign:"center"}}>
                    <thead style={{backgroundColor: "#cfd8dc"}}>
                      {/* 1st Row Sections Labels */}
                      <tr>
                        <td colSpan="1"></td>
                        {gridForm.sections.map((section, index) => {
                          let showSection = checkToShowSection(section)
                          if(showSection === true){
                            let lastSection = checkSectionOnLastChild(index)
                            return (
                              <td
                                // class={lastSection === true ? "td-head-last-child" : "td-head-style"}
                                colSpan={section.contents.length}
                              >{section.label}</td>
                            )
                          }
                        })}
                      </tr>
                      {/* 2nd Row Sections Contents labels */}
                      <tr>
                        <td 
                          // class="td-head-style-2row"
                          rowSpan="2" 
                          style={{minWidth:"85px"}}
                        >
                          Действие
                        </td>
                        {gridForm.sections.map(section =>
                          section.contents.map(contentItem => {
                            if(contentItem.show === true){
                              return (
                                <td 
                                  // class="td-head-style-2row"
                                  rowSpan="2"
                                  onClick={()=> sortDataByColumn(contentItem.name, contentItem.type)} 
                                >
                                  {contentItem.label}
                                  {sortedColumn === contentItem.name ? sortedColumnOrder === 1 ? <ArrowDropDownIcon style={{marginBottom: -8}}/> : <ArrowDropUpIcon style={{marginBottom: -8}}/> : ""}
                                </td>
                              )
                            }
                          })
                        )}
                      </tr>
                    </thead>
                    <tbody>
                      {Object.keys(docList).length !== 0 &&
                        docList.map(dataItem => (
                          <tr key={keyGen(5)} style={{"height": 30}}>
                            <td 
                              class="td-style"
                              style={{"maxWidth": 34}}
                            >
                              {gridFormButtons !== "null" &&
                                gridFormButtons.map(button => 
                                <Button
                                  class="gridFormBtn"
                                  key={button.name}
                                  name={button.name}
                                  variant="outlined"                                  
                                  value={button.name}                               
                                  onClick = {() => buttonClick(button.name, dataItem)}
                                >{button.label}
                                </Button>
                              )}
                            </td>
                            {gridForm.sections.map(section => (
                              section.contents.map(contentItem => {
                                if(contentItem.show === true){
                                  return(
                                    <td key={keyGen(4)} align="left" style={{color: "black", fontSize: 12, "text-align": "center", "border-bottom": "1px solid grey"}}>
                                      {getGridFormItems(dataItem, contentItem)}
                                    </td>
                                  )
                                }
                              })
                            ))}
                          </tr>
                        )
                      )}                    
                    </tbody>
                  </table>
                  <tfoot>
                    <tr>
                      <td style={{paddingLeft: "20px"}}>
                        <div style={{minWidth: 90, color: "black"}}>Кол-во записей</div>
                      </td>                    
                      <td style={{paddingLeft: "3px"}}>
                        <FormControl
                          variant="outlined"
                          style={{minWidth: 30}}
                        >
                          <GridSelect 
                            onChange={handleChangeRowsPerPage}
                            style={{height: 25, color: "black"}}
                            value = {size}
                            > 
                            <MenuItem value = {10}>10</MenuItem>
                            <MenuItem value = {20}>20</MenuItem>
                            <MenuItem value = {50}>50</MenuItem>
                            <MenuItem value = {100}>100</MenuItem>
                            <MenuItem value = {200}>200</MenuItem>
                            <MenuItem value = {500}>500</MenuItem>
                          </GridSelect>
                        </FormControl>
                      </td>
                      
                      <td>
                        <Tooltip title="На первую страницу">
                          <IconButton onClick={() => KeyboardArrowFirstClick()}>
                            <FirstPageIcon style={{fontSize: "large", color: "black"}}/>
                          </IconButton>
                        </Tooltip>
                      </td>
                      <td>
                        <Tooltip title="На предыдущюю страницу">
                          <IconButton onClick={() => KeyboardArrowLeftClick(page)}>
                            <ArrowBackIosIcon style={{fontSize: "medium", color: "black"}}/>
                          </IconButton>
                        </Tooltip>
                      </td>
                      <td style={{color: "black", fontSize: 16}}>
                        <input style={{maxWidth: 25}} value={page} onChange={handlePageChange}></input>
                      </td>
                      <td style={{paddingLeft: "3px"}}>
                        <Tooltip title="Перейти на указанную страницу">                              
                            <Button
                              onClick={()=> GoToPage()}
                              variant="outlined"
                              style={{
                                height: 22,
                                backgroundColor: "#D1D6D6",
                                fontSize: 12
                              }}
                            >перейти
                            </Button>
                        </Tooltip>
                      </td>
                      <td>
                        <Tooltip title="На следующюю страницу">
                          <IconButton onClick={() => KeyboardArrowRightClick(page)}>
                            <ArrowForwardIosIcon style={{fontSize: "medium", color: "black"}}/>
                          </IconButton>
                        </Tooltip>
                      </td>
                      <td class="pagination-rows-amount-style">Стр. {page} из {getPageAmount()} Общее кол-во {initialDocList.length}
                      </td>                   
                    </tr>
                  </tfoot> 
                </Paper>                  
              </Grid>
            </Grid>               
          }
          </Grid>
          <Snackbar
            open={showSnackBar}
            onClose={()=> handleCloseSnackBar()}
            autoHideDuration={1200} 
            message={snackBarMessage}
          />  
        </Grid>
      )
    }
    catch(er){
      console.log("ERROR", er)
      return <LinearProgress/>
    }
  }
};

