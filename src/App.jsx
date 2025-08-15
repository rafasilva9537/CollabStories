import './Global.css'

import { BrowserRouter, Routes, Route, Link } from "react-router-dom";

/*Importando páginas*/ 
import Stories from './pages/stories/Stories'
import Home from './pages/home/Home'

function App() {

  return (

    <BrowserRouter>
    
      <Routes>

        <Route path='/story' element={<Stories/>}/>
        <Route path='/' element={<Home/>}/>

      </Routes>

    </BrowserRouter>

  )
}

export default App;