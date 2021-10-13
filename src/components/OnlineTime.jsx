import React, { useState, useEffect } from 'react';
import { makeStyles } from '@material-ui/core/styles';

const useStyles = makeStyles((theme) => ({
  timeText: {
    fontFamily: 'Courier',
    fontSize:14,
    fontWeight: "bold",
    variant: 'body2',
    paddingLeft: theme.spacing(1),
    color:"#00838f"
    }
  }));

export default function OnlineTime() { 
  const classes = useStyles(); 
  const [seconds, setSeconds] = useState(0);
  useEffect(() => {
    const interval = setInterval(() => {
      setSeconds(seconds => seconds + 1);
    }, 1000);
    return () => clearInterval(interval);
  }, []);

  function parseDate(date){
    let convertedDate
    if(date !== undefined){
      let newDate = new Date(date) // "2020-12-31"
      let dd = String(newDate.getDate()).padStart(2, '0')
      let mm = String(newDate.getMonth() + 1).padStart(2, '0') //January is 0!
      let yyyy = newDate.getFullYear()
      let hours = newDate.getHours()
      let minutes = newDate.getMinutes()
      let seconds = newDate.getSeconds()   
      convertedDate = dd + '.' + mm + '.' + yyyy + " " + hours + ":" + minutes + ":" + seconds
    }
    return convertedDate    
  }
  let today = parseDate(new Date())

  return (
    <div className={classes.timeText}>
      {today}
    </div>
  );
}