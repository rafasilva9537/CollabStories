import { BrowserRouter, Routes, Route } from "react-router-dom";

import './styles/Global.css'

/*Pages*/
import Account from './pages/log/Account'
import Home from './pages/home/Home'
import Story from './pages/stories/Story'

function App() {

  return (

    <BrowserRouter>
    
      <Routes>

        <Route path="/" element={<Account/>}/>
        <Route path="/home" element={<Home/>}/>
        <Route path="/story" element={<Story/>}/>

      </Routes>

    </BrowserRouter>

  )

}

export default App;