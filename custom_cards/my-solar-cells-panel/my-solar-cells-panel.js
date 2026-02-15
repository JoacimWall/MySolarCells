function t(t,e,s,a){var i,r=arguments.length,o=r<3?e:null===a?a=Object.getOwnPropertyDescriptor(e,s):a;if("object"==typeof Reflect&&"function"==typeof Reflect.decorate)o=Reflect.decorate(t,e,s,a);else for(var n=t.length-1;n>=0;n--)(i=t[n])&&(o=(r<3?i(o):r>3?i(e,s,o):i(e,s))||o);return r>3&&o&&Object.defineProperty(e,s,o),o}"function"==typeof SuppressedError&&SuppressedError;
/**
 * @license
 * Copyright 2019 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */
const e=globalThis,s=e.ShadowRoot&&(void 0===e.ShadyCSS||e.ShadyCSS.nativeShadow)&&"adoptedStyleSheets"in Document.prototype&&"replace"in CSSStyleSheet.prototype,a=Symbol(),i=new WeakMap;let r=class{constructor(t,e,s){if(this._$cssResult$=!0,s!==a)throw Error("CSSResult is not constructable. Use `unsafeCSS` or `css` instead.");this.cssText=t,this.t=e}get styleSheet(){let t=this.o;const e=this.t;if(s&&void 0===t){const s=void 0!==e&&1===e.length;s&&(t=i.get(e)),void 0===t&&((this.o=t=new CSSStyleSheet).replaceSync(this.cssText),s&&i.set(e,t))}return t}toString(){return this.cssText}};const o=(t,...e)=>{const s=1===t.length?t[0]:e.reduce((e,s,a)=>e+(t=>{if(!0===t._$cssResult$)return t.cssText;if("number"==typeof t)return t;throw Error("Value passed to 'css' function must be a 'css' function result: "+t+". Use 'unsafeCSS' to pass non-literal values, but take care to ensure page security.")})(s)+t[a+1],t[0]);return new r(s,t,a)},n=s?t=>t:t=>t instanceof CSSStyleSheet?(t=>{let e="";for(const s of t.cssRules)e+=s.cssText;return(t=>new r("string"==typeof t?t:t+"",void 0,a))(e)})(t):t,{is:d,defineProperty:l,getOwnPropertyDescriptor:c,getOwnPropertyNames:h,getOwnPropertySymbols:p,getPrototypeOf:u}=Object,_=globalThis,v=_.trustedTypes,g=v?v.emptyScript:"",m=_.reactiveElementPolyfillSupport,y=(t,e)=>t,f={toAttribute(t,e){switch(e){case Boolean:t=t?g:null;break;case Object:case Array:t=null==t?t:JSON.stringify(t)}return t},fromAttribute(t,e){let s=t;switch(e){case Boolean:s=null!==t;break;case Number:s=null===t?null:Number(t);break;case Object:case Array:try{s=JSON.parse(t)}catch(t){s=null}}return s}},b=(t,e)=>!d(t,e),$={attribute:!0,type:String,converter:f,reflect:!1,useDefault:!1,hasChanged:b};
/**
 * @license
 * Copyright 2017 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */Symbol.metadata??=Symbol("metadata"),_.litPropertyMetadata??=new WeakMap;let x=class extends HTMLElement{static addInitializer(t){this._$Ei(),(this.l??=[]).push(t)}static get observedAttributes(){return this.finalize(),this._$Eh&&[...this._$Eh.keys()]}static createProperty(t,e=$){if(e.state&&(e.attribute=!1),this._$Ei(),this.prototype.hasOwnProperty(t)&&((e=Object.create(e)).wrapped=!0),this.elementProperties.set(t,e),!e.noAccessor){const s=Symbol(),a=this.getPropertyDescriptor(t,s,e);void 0!==a&&l(this.prototype,t,a)}}static getPropertyDescriptor(t,e,s){const{get:a,set:i}=c(this.prototype,t)??{get(){return this[e]},set(t){this[e]=t}};return{get:a,set(e){const r=a?.call(this);i?.call(this,e),this.requestUpdate(t,r,s)},configurable:!0,enumerable:!0}}static getPropertyOptions(t){return this.elementProperties.get(t)??$}static _$Ei(){if(this.hasOwnProperty(y("elementProperties")))return;const t=u(this);t.finalize(),void 0!==t.l&&(this.l=[...t.l]),this.elementProperties=new Map(t.elementProperties)}static finalize(){if(this.hasOwnProperty(y("finalized")))return;if(this.finalized=!0,this._$Ei(),this.hasOwnProperty(y("properties"))){const t=this.properties,e=[...h(t),...p(t)];for(const s of e)this.createProperty(s,t[s])}const t=this[Symbol.metadata];if(null!==t){const e=litPropertyMetadata.get(t);if(void 0!==e)for(const[t,s]of e)this.elementProperties.set(t,s)}this._$Eh=new Map;for(const[t,e]of this.elementProperties){const s=this._$Eu(t,e);void 0!==s&&this._$Eh.set(s,t)}this.elementStyles=this.finalizeStyles(this.styles)}static finalizeStyles(t){const e=[];if(Array.isArray(t)){const s=new Set(t.flat(1/0).reverse());for(const t of s)e.unshift(n(t))}else void 0!==t&&e.push(n(t));return e}static _$Eu(t,e){const s=e.attribute;return!1===s?void 0:"string"==typeof s?s:"string"==typeof t?t.toLowerCase():void 0}constructor(){super(),this._$Ep=void 0,this.isUpdatePending=!1,this.hasUpdated=!1,this._$Em=null,this._$Ev()}_$Ev(){this._$ES=new Promise(t=>this.enableUpdating=t),this._$AL=new Map,this._$E_(),this.requestUpdate(),this.constructor.l?.forEach(t=>t(this))}addController(t){(this._$EO??=new Set).add(t),void 0!==this.renderRoot&&this.isConnected&&t.hostConnected?.()}removeController(t){this._$EO?.delete(t)}_$E_(){const t=new Map,e=this.constructor.elementProperties;for(const s of e.keys())this.hasOwnProperty(s)&&(t.set(s,this[s]),delete this[s]);t.size>0&&(this._$Ep=t)}createRenderRoot(){const t=this.shadowRoot??this.attachShadow(this.constructor.shadowRootOptions);return((t,a)=>{if(s)t.adoptedStyleSheets=a.map(t=>t instanceof CSSStyleSheet?t:t.styleSheet);else for(const s of a){const a=document.createElement("style"),i=e.litNonce;void 0!==i&&a.setAttribute("nonce",i),a.textContent=s.cssText,t.appendChild(a)}})(t,this.constructor.elementStyles),t}connectedCallback(){this.renderRoot??=this.createRenderRoot(),this.enableUpdating(!0),this._$EO?.forEach(t=>t.hostConnected?.())}enableUpdating(t){}disconnectedCallback(){this._$EO?.forEach(t=>t.hostDisconnected?.())}attributeChangedCallback(t,e,s){this._$AK(t,s)}_$ET(t,e){const s=this.constructor.elementProperties.get(t),a=this.constructor._$Eu(t,s);if(void 0!==a&&!0===s.reflect){const i=(void 0!==s.converter?.toAttribute?s.converter:f).toAttribute(e,s.type);this._$Em=t,null==i?this.removeAttribute(a):this.setAttribute(a,i),this._$Em=null}}_$AK(t,e){const s=this.constructor,a=s._$Eh.get(t);if(void 0!==a&&this._$Em!==a){const t=s.getPropertyOptions(a),i="function"==typeof t.converter?{fromAttribute:t.converter}:void 0!==t.converter?.fromAttribute?t.converter:f;this._$Em=a;const r=i.fromAttribute(e,t.type);this[a]=r??this._$Ej?.get(a)??r,this._$Em=null}}requestUpdate(t,e,s,a=!1,i){if(void 0!==t){const r=this.constructor;if(!1===a&&(i=this[t]),s??=r.getPropertyOptions(t),!((s.hasChanged??b)(i,e)||s.useDefault&&s.reflect&&i===this._$Ej?.get(t)&&!this.hasAttribute(r._$Eu(t,s))))return;this.C(t,e,s)}!1===this.isUpdatePending&&(this._$ES=this._$EP())}C(t,e,{useDefault:s,reflect:a,wrapped:i},r){s&&!(this._$Ej??=new Map).has(t)&&(this._$Ej.set(t,r??e??this[t]),!0!==i||void 0!==r)||(this._$AL.has(t)||(this.hasUpdated||s||(e=void 0),this._$AL.set(t,e)),!0===a&&this._$Em!==t&&(this._$Eq??=new Set).add(t))}async _$EP(){this.isUpdatePending=!0;try{await this._$ES}catch(t){Promise.reject(t)}const t=this.scheduleUpdate();return null!=t&&await t,!this.isUpdatePending}scheduleUpdate(){return this.performUpdate()}performUpdate(){if(!this.isUpdatePending)return;if(!this.hasUpdated){if(this.renderRoot??=this.createRenderRoot(),this._$Ep){for(const[t,e]of this._$Ep)this[t]=e;this._$Ep=void 0}const t=this.constructor.elementProperties;if(t.size>0)for(const[e,s]of t){const{wrapped:t}=s,a=this[e];!0!==t||this._$AL.has(e)||void 0===a||this.C(e,void 0,s,a)}}let t=!1;const e=this._$AL;try{t=this.shouldUpdate(e),t?(this.willUpdate(e),this._$EO?.forEach(t=>t.hostUpdate?.()),this.update(e)):this._$EM()}catch(e){throw t=!1,this._$EM(),e}t&&this._$AE(e)}willUpdate(t){}_$AE(t){this._$EO?.forEach(t=>t.hostUpdated?.()),this.hasUpdated||(this.hasUpdated=!0,this.firstUpdated(t)),this.updated(t)}_$EM(){this._$AL=new Map,this.isUpdatePending=!1}get updateComplete(){return this.getUpdateComplete()}getUpdateComplete(){return this._$ES}shouldUpdate(t){return!0}update(t){this._$Eq&&=this._$Eq.forEach(t=>this._$ET(t,this[t])),this._$EM()}updated(t){}firstUpdated(t){}};x.elementStyles=[],x.shadowRootOptions={mode:"open"},x[y("elementProperties")]=new Map,x[y("finalized")]=new Map,m?.({ReactiveElement:x}),(_.reactiveElementVersions??=[]).push("2.1.2");
/**
 * @license
 * Copyright 2017 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */
const w=globalThis,k=t=>t,S=w.trustedTypes,A=S?S.createPolicy("lit-html",{createHTML:t=>t}):void 0,E="$lit$",D=`lit$${Math.random().toFixed(9).slice(2)}$`,P="?"+D,I=`<${P}>`,T=document,C=()=>T.createComment(""),R=t=>null===t||"object"!=typeof t&&"function"!=typeof t,F=Array.isArray,O="[ \t\n\f\r]",M=/<(?:(!--|\/[^a-zA-Z])|(\/?[a-zA-Z][^>\s]*)|(\/?$))/g,Y=/-->/g,L=/>/g,U=RegExp(`>|${O}(?:([^\\s"'>=/]+)(${O}*=${O}*(?:[^ \t\n\f\r"'\`<>=]|("|')|))|$)`,"g"),N=/'/g,z=/"/g,j=/^(?:script|style|textarea|title)$/i,K=(t=>(e,...s)=>({_$litType$:t,strings:e,values:s}))(1),B=Symbol.for("lit-noChange"),H=Symbol.for("lit-nothing"),W=new WeakMap,V=T.createTreeWalker(T,129);function q(t,e){if(!F(t)||!t.hasOwnProperty("raw"))throw Error("invalid template strings array");return void 0!==A?A.createHTML(e):e}const J=(t,e)=>{const s=t.length-1,a=[];let i,r=2===e?"<svg>":3===e?"<math>":"",o=M;for(let e=0;e<s;e++){const s=t[e];let n,d,l=-1,c=0;for(;c<s.length&&(o.lastIndex=c,d=o.exec(s),null!==d);)c=o.lastIndex,o===M?"!--"===d[1]?o=Y:void 0!==d[1]?o=L:void 0!==d[2]?(j.test(d[2])&&(i=RegExp("</"+d[2],"g")),o=U):void 0!==d[3]&&(o=U):o===U?">"===d[0]?(o=i??M,l=-1):void 0===d[1]?l=-2:(l=o.lastIndex-d[2].length,n=d[1],o=void 0===d[3]?U:'"'===d[3]?z:N):o===z||o===N?o=U:o===Y||o===L?o=M:(o=U,i=void 0);const h=o===U&&t[e+1].startsWith("/>")?" ":"";r+=o===M?s+I:l>=0?(a.push(n),s.slice(0,l)+E+s.slice(l)+D+h):s+D+(-2===l?e:h)}return[q(t,r+(t[s]||"<?>")+(2===e?"</svg>":3===e?"</math>":"")),a]};class G{constructor({strings:t,_$litType$:e},s){let a;this.parts=[];let i=0,r=0;const o=t.length-1,n=this.parts,[d,l]=J(t,e);if(this.el=G.createElement(d,s),V.currentNode=this.el.content,2===e||3===e){const t=this.el.content.firstChild;t.replaceWith(...t.childNodes)}for(;null!==(a=V.nextNode())&&n.length<o;){if(1===a.nodeType){if(a.hasAttributes())for(const t of a.getAttributeNames())if(t.endsWith(E)){const e=l[r++],s=a.getAttribute(t).split(D),o=/([.?@])?(.*)/.exec(e);n.push({type:1,index:i,name:o[2],strings:s,ctor:"."===o[1]?et:"?"===o[1]?st:"@"===o[1]?at:tt}),a.removeAttribute(t)}else t.startsWith(D)&&(n.push({type:6,index:i}),a.removeAttribute(t));if(j.test(a.tagName)){const t=a.textContent.split(D),e=t.length-1;if(e>0){a.textContent=S?S.emptyScript:"";for(let s=0;s<e;s++)a.append(t[s],C()),V.nextNode(),n.push({type:2,index:++i});a.append(t[e],C())}}}else if(8===a.nodeType)if(a.data===P)n.push({type:2,index:i});else{let t=-1;for(;-1!==(t=a.data.indexOf(D,t+1));)n.push({type:7,index:i}),t+=D.length-1}i++}}static createElement(t,e){const s=T.createElement("template");return s.innerHTML=t,s}}function Z(t,e,s=t,a){if(e===B)return e;let i=void 0!==a?s._$Co?.[a]:s._$Cl;const r=R(e)?void 0:e._$litDirective$;return i?.constructor!==r&&(i?._$AO?.(!1),void 0===r?i=void 0:(i=new r(t),i._$AT(t,s,a)),void 0!==a?(s._$Co??=[])[a]=i:s._$Cl=i),void 0!==i&&(e=Z(t,i._$AS(t,e.values),i,a)),e}class Q{constructor(t,e){this._$AV=[],this._$AN=void 0,this._$AD=t,this._$AM=e}get parentNode(){return this._$AM.parentNode}get _$AU(){return this._$AM._$AU}u(t){const{el:{content:e},parts:s}=this._$AD,a=(t?.creationScope??T).importNode(e,!0);V.currentNode=a;let i=V.nextNode(),r=0,o=0,n=s[0];for(;void 0!==n;){if(r===n.index){let e;2===n.type?e=new X(i,i.nextSibling,this,t):1===n.type?e=new n.ctor(i,n.name,n.strings,this,t):6===n.type&&(e=new it(i,this,t)),this._$AV.push(e),n=s[++o]}r!==n?.index&&(i=V.nextNode(),r++)}return V.currentNode=T,a}p(t){let e=0;for(const s of this._$AV)void 0!==s&&(void 0!==s.strings?(s._$AI(t,s,e),e+=s.strings.length-2):s._$AI(t[e])),e++}}class X{get _$AU(){return this._$AM?._$AU??this._$Cv}constructor(t,e,s,a){this.type=2,this._$AH=H,this._$AN=void 0,this._$AA=t,this._$AB=e,this._$AM=s,this.options=a,this._$Cv=a?.isConnected??!0}get parentNode(){let t=this._$AA.parentNode;const e=this._$AM;return void 0!==e&&11===t?.nodeType&&(t=e.parentNode),t}get startNode(){return this._$AA}get endNode(){return this._$AB}_$AI(t,e=this){t=Z(this,t,e),R(t)?t===H||null==t||""===t?(this._$AH!==H&&this._$AR(),this._$AH=H):t!==this._$AH&&t!==B&&this._(t):void 0!==t._$litType$?this.$(t):void 0!==t.nodeType?this.T(t):(t=>F(t)||"function"==typeof t?.[Symbol.iterator])(t)?this.k(t):this._(t)}O(t){return this._$AA.parentNode.insertBefore(t,this._$AB)}T(t){this._$AH!==t&&(this._$AR(),this._$AH=this.O(t))}_(t){this._$AH!==H&&R(this._$AH)?this._$AA.nextSibling.data=t:this.T(T.createTextNode(t)),this._$AH=t}$(t){const{values:e,_$litType$:s}=t,a="number"==typeof s?this._$AC(t):(void 0===s.el&&(s.el=G.createElement(q(s.h,s.h[0]),this.options)),s);if(this._$AH?._$AD===a)this._$AH.p(e);else{const t=new Q(a,this),s=t.u(this.options);t.p(e),this.T(s),this._$AH=t}}_$AC(t){let e=W.get(t.strings);return void 0===e&&W.set(t.strings,e=new G(t)),e}k(t){F(this._$AH)||(this._$AH=[],this._$AR());const e=this._$AH;let s,a=0;for(const i of t)a===e.length?e.push(s=new X(this.O(C()),this.O(C()),this,this.options)):s=e[a],s._$AI(i),a++;a<e.length&&(this._$AR(s&&s._$AB.nextSibling,a),e.length=a)}_$AR(t=this._$AA.nextSibling,e){for(this._$AP?.(!1,!0,e);t!==this._$AB;){const e=k(t).nextSibling;k(t).remove(),t=e}}setConnected(t){void 0===this._$AM&&(this._$Cv=t,this._$AP?.(t))}}class tt{get tagName(){return this.element.tagName}get _$AU(){return this._$AM._$AU}constructor(t,e,s,a,i){this.type=1,this._$AH=H,this._$AN=void 0,this.element=t,this.name=e,this._$AM=a,this.options=i,s.length>2||""!==s[0]||""!==s[1]?(this._$AH=Array(s.length-1).fill(new String),this.strings=s):this._$AH=H}_$AI(t,e=this,s,a){const i=this.strings;let r=!1;if(void 0===i)t=Z(this,t,e,0),r=!R(t)||t!==this._$AH&&t!==B,r&&(this._$AH=t);else{const a=t;let o,n;for(t=i[0],o=0;o<i.length-1;o++)n=Z(this,a[s+o],e,o),n===B&&(n=this._$AH[o]),r||=!R(n)||n!==this._$AH[o],n===H?t=H:t!==H&&(t+=(n??"")+i[o+1]),this._$AH[o]=n}r&&!a&&this.j(t)}j(t){t===H?this.element.removeAttribute(this.name):this.element.setAttribute(this.name,t??"")}}class et extends tt{constructor(){super(...arguments),this.type=3}j(t){this.element[this.name]=t===H?void 0:t}}class st extends tt{constructor(){super(...arguments),this.type=4}j(t){this.element.toggleAttribute(this.name,!!t&&t!==H)}}class at extends tt{constructor(t,e,s,a,i){super(t,e,s,a,i),this.type=5}_$AI(t,e=this){if((t=Z(this,t,e,0)??H)===B)return;const s=this._$AH,a=t===H&&s!==H||t.capture!==s.capture||t.once!==s.once||t.passive!==s.passive,i=t!==H&&(s===H||a);a&&this.element.removeEventListener(this.name,this,s),i&&this.element.addEventListener(this.name,this,t),this._$AH=t}handleEvent(t){"function"==typeof this._$AH?this._$AH.call(this.options?.host??this.element,t):this._$AH.handleEvent(t)}}class it{constructor(t,e,s){this.element=t,this.type=6,this._$AN=void 0,this._$AM=e,this.options=s}get _$AU(){return this._$AM._$AU}_$AI(t){Z(this,t)}}const rt=w.litHtmlPolyfillSupport;rt?.(G,X),(w.litHtmlVersions??=[]).push("3.3.2");const ot=globalThis;
/**
 * @license
 * Copyright 2017 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */class nt extends x{constructor(){super(...arguments),this.renderOptions={host:this},this._$Do=void 0}createRenderRoot(){const t=super.createRenderRoot();return this.renderOptions.renderBefore??=t.firstChild,t}update(t){const e=this.render();this.hasUpdated||(this.renderOptions.isConnected=this.isConnected),super.update(t),this._$Do=((t,e,s)=>{const a=s?.renderBefore??e;let i=a._$litPart$;if(void 0===i){const t=s?.renderBefore??null;a._$litPart$=i=new X(e.insertBefore(C(),t),t,void 0,s??{})}return i._$AI(t),i})(e,this.renderRoot,this.renderOptions)}connectedCallback(){super.connectedCallback(),this._$Do?.setConnected(!0)}disconnectedCallback(){super.disconnectedCallback(),this._$Do?.setConnected(!1)}render(){return B}}nt._$litElement$=!0,nt.finalized=!0,ot.litElementHydrateSupport?.({LitElement:nt});const dt=ot.litElementPolyfillSupport;dt?.({LitElement:nt}),(ot.litElementVersions??=[]).push("4.2.2");
/**
 * @license
 * Copyright 2017 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */
const lt=t=>(e,s)=>{void 0!==s?s.addInitializer(()=>{customElements.define(t,e)}):customElements.define(t,e)},ct={attribute:!0,type:String,converter:f,reflect:!1,hasChanged:b},ht=(t=ct,e,s)=>{const{kind:a,metadata:i}=s;let r=globalThis.litPropertyMetadata.get(i);if(void 0===r&&globalThis.litPropertyMetadata.set(i,r=new Map),"setter"===a&&((t=Object.create(t)).wrapped=!0),r.set(s.name,t),"accessor"===a){const{name:a}=s;return{set(s){const i=e.get.call(this);e.set.call(this,s),this.requestUpdate(a,i,t,!0,s)},init(e){return void 0!==e&&this.C(a,void 0,t,e),e}}}if("setter"===a){const{name:a}=s;return function(s){const i=this[a];e.call(this,s),this.requestUpdate(a,i,t,!0,s)}}throw Error("Unsupported decorator location: "+a)};
/**
 * @license
 * Copyright 2017 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */function pt(t){return(e,s)=>"object"==typeof s?ht(t,e,s):((t,e,s)=>{const a=e.hasOwnProperty(s);return e.constructor.createProperty(s,t),a?Object.getOwnPropertyDescriptor(e,s):void 0})(t,e,s)}
/**
 * @license
 * Copyright 2017 Google LLC
 * SPDX-License-Identifier: BSD-3-Clause
 */function ut(t){return pt({...t,state:!0,attribute:!1})}const _t=o`
  :host {
    display: block;
    padding: 16px;
    --mdc-theme-primary: var(--primary-color);
  }

  .header {
    display: flex;
    align-items: center;
    justify-content: space-between;
    margin-bottom: 16px;
  }

  .header h1 {
    margin: 0;
    font-size: 1.5em;
    font-weight: 400;
    color: var(--primary-text-color);
  }

  .tabs {
    display: flex;
    border-bottom: 1px solid var(--divider-color);
    margin-bottom: 16px;
  }

  .tab {
    padding: 12px 20px;
    cursor: pointer;
    border-bottom: 2px solid transparent;
    color: var(--secondary-text-color);
    font-size: 0.95em;
    font-weight: 500;
    transition: color 0.2s, border-color 0.2s;
    background: none;
    border-top: none;
    border-left: none;
    border-right: none;
  }

  .tab:hover {
    color: var(--primary-text-color);
  }

  .tab.active {
    color: var(--primary-color);
    border-bottom-color: var(--primary-color);
  }
`,vt=o`
  .card {
    background: var(--ha-card-background, var(--card-background-color, white));
    border-radius: var(--ha-card-border-radius, 12px);
    box-shadow: var(--ha-card-box-shadow, 0 2px 6px rgba(0,0,0,0.1));
    padding: 16px;
    margin-bottom: 16px;
  }

  .card h3 {
    margin: 0 0 12px 0;
    font-size: 1.1em;
    font-weight: 500;
    color: var(--primary-text-color);
  }

  .stats-grid {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(150px, 1fr));
    gap: 12px;
  }

  .stat-item {
    text-align: center;
    padding: 12px 8px;
    border-radius: 8px;
    background: var(--primary-background-color);
  }

  .stat-label {
    font-size: 0.8em;
    color: var(--secondary-text-color);
    margin-bottom: 4px;
  }

  .stat-value {
    font-size: 1.3em;
    font-weight: 600;
    color: var(--primary-text-color);
  }
`,gt=o`
  .table-controls {
    display: flex;
    gap: 12px;
    align-items: flex-end;
    flex-wrap: wrap;
    margin-bottom: 16px;
  }

  .input-group {
    display: flex;
    flex-direction: column;
    gap: 4px;
  }

  .input-group label {
    font-size: 0.8em;
    color: var(--secondary-text-color);
  }

  .input-group input {
    padding: 8px 12px;
    border: 1px solid var(--divider-color);
    border-radius: 6px;
    background: var(--card-background-color, white);
    color: var(--primary-text-color);
    font-size: 0.9em;
  }

  button.btn {
    padding: 8px 16px;
    border: none;
    border-radius: 6px;
    background: var(--primary-color);
    color: var(--text-primary-color, white);
    cursor: pointer;
    font-size: 0.9em;
    font-weight: 500;
  }

  button.btn:hover {
    opacity: 0.9;
  }

  button.btn:disabled {
    opacity: 0.5;
    cursor: not-allowed;
  }

  table {
    width: 100%;
    border-collapse: collapse;
    font-size: 0.85em;
  }

  th {
    text-align: left;
    padding: 8px 6px;
    border-bottom: 2px solid var(--divider-color);
    color: var(--secondary-text-color);
    font-weight: 500;
    white-space: nowrap;
  }

  td {
    padding: 6px;
    border-bottom: 1px solid var(--divider-color);
  }

  .table-wrapper {
    overflow-x: auto;
  }

  .pagination {
    display: flex;
    align-items: center;
    justify-content: center;
    gap: 16px;
    margin-top: 12px;
    font-size: 0.9em;
    color: var(--secondary-text-color);
  }

  .summary-row {
    background: var(--primary-background-color);
    font-weight: 600;
  }

  .loading {
    text-align: center;
    color: var(--secondary-text-color);
    padding: 40px 16px;
    font-style: italic;
  }

  .no-data {
    text-align: center;
    color: var(--secondary-text-color);
    padding: 40px 16px;
    font-style: italic;
  }
`,mt=o`
  .period-nav {
    display: flex;
    align-items: center;
    gap: 8px;
    margin-bottom: 16px;
    flex-wrap: wrap;
  }

  .period-tabs {
    display: flex;
    gap: 2px;
    background: var(--divider-color);
    border-radius: 8px;
    padding: 2px;
  }

  .period-tab {
    padding: 6px 14px;
    border: none;
    background: transparent;
    color: var(--secondary-text-color);
    cursor: pointer;
    border-radius: 6px;
    font-size: 0.85em;
    font-weight: 500;
    transition: background 0.15s, color 0.15s;
  }

  .period-tab:hover {
    color: var(--primary-text-color);
  }

  .period-tab.active {
    background: var(--primary-color);
    color: var(--text-primary-color, white);
  }

  .nav-arrow {
    padding: 4px 10px;
    border: 1px solid var(--divider-color);
    background: var(--ha-card-background, var(--card-background-color, white));
    color: var(--primary-text-color);
    cursor: pointer;
    border-radius: 6px;
    font-size: 1em;
    line-height: 1;
  }

  .nav-arrow:hover {
    background: var(--primary-background-color);
  }

  .period-label {
    font-weight: 600;
    font-size: 0.9em;
    color: var(--primary-text-color);
    min-width: 120px;
    text-align: center;
  }

  .fakta-columns {
    display: grid;
    grid-template-columns: 1fr 1fr 1fr;
    gap: 16px;
    align-items: start;
  }

  @media (max-width: 900px) {
    .fakta-columns {
      grid-template-columns: 1fr;
    }
  }

  .fakta-card {
    background: var(--ha-card-background, var(--card-background-color, white));
    border-radius: var(--ha-card-border-radius, 12px);
    box-shadow: var(--ha-card-box-shadow, 0 2px 6px rgba(0,0,0,0.1));
    padding: 16px;
  }

  .fakta-card h3 {
    margin: 0 0 12px 0;
    font-size: 1em;
    font-weight: 600;
    color: var(--primary-text-color);
  }

  .fakta-row {
    display: flex;
    justify-content: space-between;
    padding: 4px 0;
    font-size: 0.85em;
  }

  .fakta-row .label {
    color: var(--secondary-text-color);
  }

  .fakta-row .value {
    font-weight: 500;
    color: var(--primary-text-color);
    font-variant-numeric: tabular-nums;
  }

  .fakta-separator {
    border: none;
    border-top: 1px solid var(--divider-color);
    margin: 6px 0;
  }

  .fakta-summary {
    font-weight: 600;
    font-size: 0.9em;
  }

  .fakta-summary .value {
    font-weight: 700;
  }

  .sim-section {
    margin-bottom: 16px;
  }

  .sim-toggle-group {
    display: flex;
    gap: 2px;
    background: var(--divider-color);
    border-radius: 8px;
    padding: 2px;
    margin-bottom: 10px;
  }

  .sim-toggle {
    flex: 1;
    padding: 6px 10px;
    border: none;
    background: transparent;
    color: var(--secondary-text-color);
    cursor: pointer;
    border-radius: 6px;
    font-size: 0.8em;
    font-weight: 500;
    transition: background 0.15s, color 0.15s;
  }

  .sim-toggle.active {
    background: var(--primary-color);
    color: var(--text-primary-color, white);
  }

  .sim-slider-row {
    display: flex;
    align-items: center;
    gap: 8px;
    margin-bottom: 10px;
  }

  .sim-slider-row input[type="range"] {
    flex: 1;
  }

  .sim-slider-row .slider-value {
    min-width: 50px;
    text-align: right;
    font-size: 0.85em;
    font-weight: 500;
  }

  .sim-checkbox {
    display: flex;
    align-items: center;
    gap: 6px;
    margin-bottom: 10px;
    font-size: 0.85em;
    color: var(--secondary-text-color);
  }

  .sim-checkbox input {
    margin: 0;
  }

  button.sim-btn {
    width: 100%;
    padding: 8px 16px;
    border: none;
    border-radius: 6px;
    background: var(--primary-color);
    color: var(--text-primary-color, white);
    cursor: pointer;
    font-size: 0.85em;
    font-weight: 500;
    margin-bottom: 16px;
  }

  button.sim-btn:hover {
    opacity: 0.9;
  }

  button.sim-btn:disabled {
    opacity: 0.5;
    cursor: not-allowed;
  }

  .loading {
    text-align: center;
    color: var(--secondary-text-color);
    padding: 40px 16px;
    font-style: italic;
  }

  .no-data {
    text-align: center;
    color: var(--secondary-text-color);
    padding: 40px 16px;
    font-style: italic;
  }
`,yt=[{key:"own_use_kwh",label:"Own Use",color:"#4285f4",unit:"kWh"},{key:"sold_kwh",label:"Sold",color:"#34a853",unit:"kWh"},{key:"own_use_sek",label:"Own Use",color:"#8ab4f8",unit:"SEK"},{key:"sold_sek",label:"Sold",color:"#f4a742",unit:"SEK"}],ft=[{key:"today",label:"Today"},{key:"this_week",label:"This Week"},{key:"this_month",label:"This Month"},{key:"this_year",label:"This Year"}];let bt=class extends nt{render(){if(!this.data)return H;const t=this._getMax("kwh"),e=this._getMax("sek");return K`
      <div class="chart-container">
        ${ft.map(s=>this._renderPeriod(s,this.data[s.key],t,e))}
      </div>
      <div class="legend">
        ${yt.map(t=>K`
            <div class="legend-item">
              <div class="legend-swatch" style="background:${t.color}"></div>
              ${t.label} (${t.unit})
            </div>
          `)}
      </div>
    `}_renderPeriod(t,e,s,a){return K`
      <div class="period-column">
        <div class="period-label">${t.label}</div>
        <div class="bars">
          ${yt.map(t=>{const i=e[t.key],r="kWh"===t.unit?s:a;return K`
              <div class="bar-wrapper">
                <div
                  class="bar"
                  style="height:${r>0?i/r*100:0}%;background:${t.color}"
                ></div>
                <div class="bar-value">${this._formatValue(i,t.unit)}</div>
              </div>
            `})}
        </div>
      </div>
    `}_getMax(t){if(!this.data)return 0;const e="kwh"===t?["own_use_kwh","sold_kwh"]:["own_use_sek","sold_sek"];let s=0;for(const t of ft){const a=this.data[t.key];for(const t of e)a[t]>s&&(s=a[t])}return s}_formatValue(t,e){return t>=1e3?`${(t/1e3).toFixed(1)}k`:t>=10?t.toFixed(0):t.toFixed(1)}};bt.styles=o`
    :host {
      display: block;
    }

    .chart-container {
      display: grid;
      grid-template-columns: repeat(4, 1fr);
      gap: 16px;
    }

    @media (max-width: 600px) {
      .chart-container {
        grid-template-columns: repeat(2, 1fr);
      }
    }

    .period-column {
      display: flex;
      flex-direction: column;
      align-items: center;
    }

    .period-label {
      font-size: 0.85em;
      font-weight: 500;
      color: var(--primary-text-color);
      margin-bottom: 8px;
    }

    .bars {
      display: flex;
      gap: 4px;
      align-items: flex-end;
      height: 120px;
      width: 100%;
      justify-content: center;
    }

    .bar-wrapper {
      display: flex;
      flex-direction: column;
      align-items: center;
      flex: 1;
      max-width: 36px;
      height: 100%;
      justify-content: flex-end;
    }

    .bar {
      width: 100%;
      border-radius: 3px 3px 0 0;
      min-height: 2px;
      transition: height 0.3s ease;
    }

    .bar-value {
      font-size: 0.65em;
      color: var(--secondary-text-color);
      margin-top: 4px;
      white-space: nowrap;
      text-align: center;
    }

    .legend {
      display: flex;
      justify-content: center;
      gap: 16px;
      margin-top: 16px;
      flex-wrap: wrap;
    }

    .legend-item {
      display: flex;
      align-items: center;
      gap: 4px;
      font-size: 0.75em;
      color: var(--secondary-text-color);
    }

    .legend-swatch {
      width: 10px;
      height: 10px;
      border-radius: 2px;
    }
  `,t([pt({attribute:!1})],bt.prototype,"data",void 0),bt=t([lt("period-summary-chart")],bt);let $t=class extends nt{constructor(){super(...arguments),this.entryId="",this._data=null,this._periodData=null,this._loading=!1,this._error=""}updated(t){t.has("hass")&&this.hass&&this.entryId&&!this._data&&!this._loading&&this._fetchData()}async _fetchData(){if(this.hass&&this.entryId){this._loading=!0,this._error="";try{const[t,e]=await Promise.all([this.hass.callWS({type:"my_solar_cells/get_overview",entry_id:this.entryId}),this.hass.callWS({type:"my_solar_cells/get_period_summaries",entry_id:this.entryId})]);this._data=t,this._periodData=e}catch(t){this._error=t.message||"Failed to fetch data"}this._loading=!1}}render(){if(this._loading)return K`<div class="loading">Loading overview...</div>`;if(this._error)return K`<div class="no-data">Error: ${this._error}</div>`;if(!this._data)return K`<div class="no-data">No data available</div>`;const t=this._data;return K`
      <div class="card">
        <h3>Database Summary</h3>
        <div class="stats-grid">
          <div class="stat-item">
            <div class="stat-label">Last Tibber Sync</div>
            <div class="stat-value">${t.last_tibber_sync?this._formatTimestamp(t.last_tibber_sync):"Never"}</div>
          </div>
          <div class="stat-item">
            <div class="stat-label">Hourly Records</div>
            <div class="stat-value">${t.hourly_record_count.toLocaleString()}</div>
          </div>
          <div class="stat-item">
            <div class="stat-label">First Record</div>
            <div class="stat-value">${t.first_timestamp?this._formatDate(t.first_timestamp):"N/A"}</div>
          </div>
          <div class="stat-item">
            <div class="stat-label">Last Record</div>
            <div class="stat-value">${t.last_timestamp?this._formatDate(t.last_timestamp):"N/A"}</div>
          </div>
        </div>
      </div>

      ${this._periodData?K`
            <div class="card">
              <h3>Energy Summary</h3>
              <period-summary-chart .data=${this._periodData}></period-summary-chart>
            </div>
          `:H}

      ${this._renderYearlyParams(t.yearly_params)}
    `}_renderYearlyParams(t){const e=Object.keys(t).sort();return 0===e.length?H:K`
      <div class="card">
        <h3>Yearly Financial Parameters</h3>
        <div class="table-wrapper">
          <table>
            <thead>
              <tr>
                <th>Year</th>
                <th>Tax Reduction</th>
                <th>Grid Comp.</th>
                <th>Transfer Fee</th>
                <th>Energy Tax</th>
                <th>Installed kW</th>
              </tr>
            </thead>
            <tbody>
              ${e.map(e=>{const s=t[e];return K`
                  <tr>
                    <td>${e}</td>
                    <td>${this._fmt(s.tax_reduction)}</td>
                    <td>${this._fmt(s.grid_compensation)}</td>
                    <td>${this._fmt(s.transfer_fee)}</td>
                    <td>${this._fmt(s.energy_tax)}</td>
                    <td>${null!=s.installed_kw?s.installed_kw:"-"}</td>
                  </tr>
                `})}
            </tbody>
          </table>
        </div>
      </div>
    `}_fmt(t){return null!=t?t.toFixed(3):"-"}_formatTimestamp(t){try{return new Date(t).toLocaleString("sv-SE")}catch{return t}}_formatDate(t){try{return t.substring(0,10)}catch{return t}}};$t.styles=[vt,gt],t([pt({attribute:!1})],$t.prototype,"hass",void 0),t([pt()],$t.prototype,"entryId",void 0),t([ut()],$t.prototype,"_data",void 0),t([ut()],$t.prototype,"_periodData",void 0),t([ut()],$t.prototype,"_loading",void 0),t([ut()],$t.prototype,"_error",void 0),$t=t([lt("overview-view")],$t);const xt=50;let wt=class extends nt{constructor(){super(...arguments),this.entryId="",this._startDate="",this._endDate="",this._records=[],this._totalCount=0,this._offset=0,this._loading=!1,this._error=""}connectedCallback(){super.connectedCallback();const t=(new Date).toISOString().substring(0,10);this._startDate=t,this._endDate=t}render(){return K`
      <div class="card">
        <h3>Hourly Energy Records</h3>
        <div class="table-controls">
          <div class="input-group">
            <label>Start Date</label>
            <input
              type="date"
              .value=${this._startDate}
              @change=${t=>{this._startDate=t.target.value}}
            />
          </div>
          <div class="input-group">
            <label>End Date</label>
            <input
              type="date"
              .value=${this._endDate}
              @change=${t=>{this._endDate=t.target.value}}
            />
          </div>
          <button class="btn" @click=${this._fetch} ?disabled=${this._loading}>
            ${this._loading?"Loading...":"Load"}
          </button>
        </div>

        ${this._error?K`<div class="no-data">Error: ${this._error}</div>`:""}
        ${this._records.length>0?this._renderTable():""}
        ${this._loading||0!==this._records.length||this._error?"":K`<div class="no-data">
              Select a date range and click Load
            </div>`}
      </div>
    `}_renderTable(){const t=Math.ceil(this._totalCount/xt),e=Math.floor(this._offset/xt)+1;return K`
      <div class="table-wrapper">
        <table>
          <thead>
            <tr>
              <th>Timestamp</th>
              <th>Purchased kWh</th>
              <th>Cost SEK</th>
              <th>Sold kWh</th>
              <th>Profit SEK</th>
              <th>Own Use kWh</th>
              <th>Saved SEK</th>
              <th>Price Level</th>
              <th>Synced</th>
              <th>Enriched</th>
            </tr>
          </thead>
          <tbody>
            ${this._records.map(t=>K`
                <tr>
                  <td>${this._formatTs(t.timestamp)}</td>
                  <td>${t.purchased.toFixed(3)}</td>
                  <td>${t.purchased_cost.toFixed(2)}</td>
                  <td>${t.production_sold.toFixed(3)}</td>
                  <td>${t.production_sold_profit.toFixed(2)}</td>
                  <td>${t.production_own_use.toFixed(3)}</td>
                  <td>${t.production_own_use_profit.toFixed(2)}</td>
                  <td>${t.price_level||"-"}</td>
                  <td>${t.synced?"Yes":"No"}</td>
                  <td>${t.sensor_enriched?"Yes":"No"}</td>
                </tr>
              `)}
          </tbody>
        </table>
      </div>
      <div class="pagination">
        <button
          class="btn"
          ?disabled=${0===this._offset||this._loading}
          @click=${this._prevPage}
        >
          Prev
        </button>
        <span>Page ${e} of ${t} (${this._totalCount} records)</span>
        <button
          class="btn"
          ?disabled=${this._offset+xt>=this._totalCount||this._loading}
          @click=${this._nextPage}
        >
          Next
        </button>
      </div>
    `}async _fetch(){if(this.hass&&this.entryId&&this._startDate&&this._endDate){this._loading=!0,this._error="";try{const t=`${this._startDate}T00:00:00`,e=new Date(this._endDate);e.setDate(e.getDate()+1);const s=`${e.toISOString().substring(0,10)}T00:00:00`,a=await this.hass.callWS({type:"my_solar_cells/get_hourly_energy",entry_id:this.entryId,start_date:t,end_date:s,offset:this._offset,limit:xt});this._records=a.records,this._totalCount=a.total_count}catch(t){this._error=t.message||"Failed to fetch data",this._records=[],this._totalCount=0}this._loading=!1}}_prevPage(){this._offset=Math.max(0,this._offset-xt),this._fetch()}_nextPage(){this._offset+=xt,this._fetch()}_formatTs(t){try{return t.replace("T"," ").substring(0,19)}catch{return t}}};wt.styles=[vt,gt],t([pt({attribute:!1})],wt.prototype,"hass",void 0),t([pt()],wt.prototype,"entryId",void 0),t([ut()],wt.prototype,"_startDate",void 0),t([ut()],wt.prototype,"_endDate",void 0),t([ut()],wt.prototype,"_records",void 0),t([ut()],wt.prototype,"_totalCount",void 0),t([ut()],wt.prototype,"_offset",void 0),t([ut()],wt.prototype,"_loading",void 0),t([ut()],wt.prototype,"_error",void 0),wt=t([lt("hourly-energy-view")],wt);let kt=class extends nt{constructor(){super(...arguments),this.entryId="",this._sensors=[],this._loading=!1,this._error=""}updated(t){t.has("hass")&&this.hass&&this.entryId&&!this._sensors.length&&!this._loading&&this._fetchData()}async _fetchData(){if(this.hass&&this.entryId){this._loading=!0,this._error="";try{const t=await this.hass.callWS({type:"my_solar_cells/get_sensor_config",entry_id:this.entryId});this._sensors=t.sensors}catch(t){this._error=t.message||"Failed to fetch sensor config"}this._loading=!1}}render(){if(this._loading)return K`<div class="loading">Loading sensor configuration...</div>`;if(this._error)return K`<div class="no-data">Error: ${this._error}</div>`;this._sensors.filter(t=>t.entity_id);const t=this._sensors.filter(t=>!t.entity_id);return K`
      <div class="info-box">
        Only the <strong>production</strong> sensor is required — it is used to
        calculate <strong>production_own_use</strong> (total production minus
        grid export). All other data (grid export, grid import) comes from the
        <strong>Tibber API</strong> by default. You can optionally override
        grid export/import with HA sensors for higher accuracy, and add battery
        sensors if you have a battery. Sensors are configured in the
        integration setup flow.
      </div>

      <div class="card">
        <h3>Sensor Configuration</h3>
        <div class="table-wrapper">
          <table>
            <thead>
              <tr>
                <th>Status</th>
                <th>Role</th>
                <th>Description</th>
                <th>Entity ID</th>
                <th>Current State</th>
                <th>Last Stored Reading</th>
              </tr>
            </thead>
            <tbody>
              ${this._sensors.map(t=>this._renderRow(t))}
            </tbody>
          </table>
        </div>
      </div>

      ${t.some(t=>"production"===t.role)?K`
            <div class="card">
              <h3>Required Sensor Missing</h3>
              <p style="color: var(--error-color, #f44336); font-size: 0.9em;">
                The <strong>production</strong> sensor is not configured.
                Without it, <strong>production_own_use</strong> cannot be
                calculated. Please configure it in the integration setup flow.
              </p>
            </div>
          `:H}

      <div style="margin-top: 12px;">
        <button class="btn" @click=${this._fetchData} ?disabled=${this._loading}>
          Refresh
        </button>
      </div>
    `}_isRequired(t){return"production"===t}_getFallbackLabel(t){return"grid_export"===t||"grid_import"===t?"Using Tibber API":"Not configured"}_renderRow(t){const e=!!t.entity_id,s=this._isRequired(t.role);return K`
      <tr>
        <td>
          <span class="status-dot ${e?"configured":s?"missing":"optional"}"></span>
        </td>
        <td>
          <strong>${t.role}</strong>
          ${s?K`<span class="required-badge">Required</span>`:K`<span class="optional-badge">Optional</span>`}
        </td>
        <td>${t.description}</td>
        <td>
          ${e?K`<span class="entity-id">${t.entity_id}</span>`:K`<span class="fallback-label">${this._getFallbackLabel(t.role)}</span>`}
        </td>
        <td>
          ${null!=t.current_state?t.current_state:K`<span class="not-configured">-</span>`}
        </td>
        <td>
          ${null!=t.last_stored_reading?t.last_stored_reading.toFixed(3):K`<span class="not-configured">-</span>`}
        </td>
      </tr>
    `}};kt.styles=[vt,gt,o`
      .status-dot {
        display: inline-block;
        width: 10px;
        height: 10px;
        border-radius: 50%;
        margin-right: 8px;
      }
      .status-dot.configured {
        background: var(--success-color, #4caf50);
      }
      .status-dot.missing {
        background: var(--error-color, #f44336);
      }
      .status-dot.optional {
        background: var(--warning-color, #ff9800);
      }
      .fallback-label {
        color: var(--secondary-text-color);
        font-size: 0.85em;
        font-style: italic;
      }
      .required-badge {
        display: inline-block;
        font-size: 0.7em;
        padding: 1px 6px;
        border-radius: 4px;
        background: var(--error-color, #f44336);
        color: white;
        font-weight: 500;
        margin-left: 6px;
        vertical-align: middle;
      }
      .optional-badge {
        display: inline-block;
        font-size: 0.7em;
        padding: 1px 6px;
        border-radius: 4px;
        background: var(--secondary-text-color);
        color: white;
        font-weight: 500;
        margin-left: 6px;
        vertical-align: middle;
      }
      .entity-id {
        font-family: monospace;
        font-size: 0.85em;
        color: var(--primary-text-color);
      }
      .not-configured {
        color: var(--secondary-text-color);
        font-style: italic;
      }
      .info-box {
        background: var(--primary-background-color);
        border-radius: 8px;
        padding: 12px 16px;
        margin-bottom: 16px;
        font-size: 0.9em;
        color: var(--secondary-text-color);
        line-height: 1.5;
      }
    `],t([pt({attribute:!1})],kt.prototype,"hass",void 0),t([pt()],kt.prototype,"entryId",void 0),t([ut()],kt.prototype,"_sensors",void 0),t([ut()],kt.prototype,"_loading",void 0),t([ut()],kt.prototype,"_error",void 0),kt=t([lt("sensors-view")],kt);const St={tax_reduction:.6,grid_compensation:.078,transfer_fee:.3,energy_tax:.49,installed_kw:10};let At=class extends nt{constructor(){super(...arguments),this.entryId="",this._params={},this._loading=!1,this._fetched=!1,this._error="",this._editingYear=null,this._editValues={...St},this._newYear="",this._saving=!1,this._minYear=0,this._maxYear=0}updated(t){(t.has("hass")||t.has("entryId"))&&this.hass&&this.entryId&&!this._loading&&!this._fetched&&this._fetchData()}async _fetchData(){if(this.hass&&this.entryId){this._loading=!0,this._error="";try{const t=await this.hass.callWS({type:"my_solar_cells/get_yearly_params",entry_id:this.entryId});this._params=t.yearly_params,t.first_timestamp&&(this._minYear=new Date(t.first_timestamp).getFullYear()),t.last_timestamp&&(this._maxYear=new Date(t.last_timestamp).getFullYear())}catch(t){this._error=t.message||"Failed to fetch yearly params"}this._loading=!1,this._fetched=!0}}render(){if(this._loading&&!this._fetched)return K`<div class="loading">Loading yearly parameters...</div>`;if(this._error)return K`<div class="no-data">Error: ${this._error}</div>`;if(!this._fetched)return K`<div class="no-data">Waiting for data...</div>`;const t=Object.keys(this._params).sort();return K`
      <div class="card">
        <h3>Yearly Financial Parameters</h3>

        <div class="add-year-row">
          <div class="input-group">
            <label>Add Year</label>
            <select
              .value=${this._newYear}
              @change=${t=>this._newYear=t.target.value}
            >
              <option value="">Select year...</option>
              ${this._getAvailableYears().map(t=>K`<option value=${t}>${t}</option>`)}
            </select>
          </div>
          <button class="btn" @click=${this._addYear}>Add</button>
        </div>

        ${0===t.length?K`<div class="no-data">
              No yearly parameters configured yet. Add a year above.
            </div>`:K`
              <div class="table-wrapper">
                <table>
                  <thead>
                    <tr>
                      <th>Year</th>
                      <th>Tax Reduction</th>
                      <th>Grid Comp.</th>
                      <th>Transfer Fee</th>
                      <th>Energy Tax</th>
                      <th>Installed kW</th>
                    </tr>
                  </thead>
                  <tbody>
                    ${t.map(t=>{const e=this._params[t],s=this._editingYear===t;return K`
                        <tr
                          class="clickable ${s?"selected":""}"
                          @click=${()=>this._startEdit(t)}
                        >
                          <td>${t}</td>
                          <td>${this._fmt(e.tax_reduction)}</td>
                          <td>${this._fmt(e.grid_compensation)}</td>
                          <td>${this._fmt(e.transfer_fee)}</td>
                          <td>${this._fmt(e.energy_tax)}</td>
                          <td>${null!=e.installed_kw?e.installed_kw:"-"}</td>
                        </tr>
                      `})}
                  </tbody>
                </table>
              </div>
            `}
      </div>

      ${null!=this._editingYear?this._renderEditForm():H}
    `}_renderEditForm(){const t=this._editValues;return K`
      <div class="card">
        <h3>Edit ${this._editingYear}</h3>
        <div class="edit-form">
          <div class="input-group">
            <label>Tax Reduction (SEK/kWh)</label>
            <input
              type="number"
              step="0.001"
              .value=${String(t.tax_reduction??"")}
              @input=${t=>this._editValues={...this._editValues,tax_reduction:parseFloat(t.target.value)}}
            />
          </div>
          <div class="input-group">
            <label>Grid Compensation (SEK/kWh)</label>
            <input
              type="number"
              step="0.001"
              .value=${String(t.grid_compensation??"")}
              @input=${t=>this._editValues={...this._editValues,grid_compensation:parseFloat(t.target.value)}}
            />
          </div>
          <div class="input-group">
            <label>Transfer Fee (SEK/kWh)</label>
            <input
              type="number"
              step="0.001"
              .value=${String(t.transfer_fee??"")}
              @input=${t=>this._editValues={...this._editValues,transfer_fee:parseFloat(t.target.value)}}
            />
          </div>
          <div class="input-group">
            <label>Energy Tax (SEK/kWh)</label>
            <input
              type="number"
              step="0.001"
              .value=${String(t.energy_tax??"")}
              @input=${t=>this._editValues={...this._editValues,energy_tax:parseFloat(t.target.value)}}
            />
          </div>
          <div class="input-group">
            <label>Installed kW</label>
            <input
              type="number"
              step="0.01"
              .value=${String(t.installed_kw??"")}
              @input=${t=>this._editValues={...this._editValues,installed_kw:parseFloat(t.target.value)}}
            />
          </div>
        </div>
        <div class="form-actions">
          <button class="btn" ?disabled=${this._saving} @click=${this._save}>
            ${this._saving?"Saving...":"Save"}
          </button>
          <button class="btn btn-secondary" @click=${this._cancelEdit}>
            Cancel
          </button>
          <button class="btn btn-danger" @click=${this._delete}>Delete</button>
        </div>
      </div>
    `}_fmt(t){return null!=t?t.toFixed(3):"-"}_startEdit(t){this._editingYear=t;const e=this._params[t]||{};this._editValues={tax_reduction:e.tax_reduction??St.tax_reduction,grid_compensation:e.grid_compensation??St.grid_compensation,transfer_fee:e.transfer_fee??St.transfer_fee,energy_tax:e.energy_tax??St.energy_tax,installed_kw:e.installed_kw??St.installed_kw}}_cancelEdit(){this._editingYear=null}_getAvailableYears(){const t=[];for(let e=this._minYear;e<=this._maxYear;e++)this._params[String(e)]||t.push(e);return t}_addYear(){const t=parseInt(this._newYear,10);if(isNaN(t))return;const e=String(t);if(this._params[e])return this._startEdit(e),void(this._newYear="");const s=Object.keys(this._params).filter(t=>t<e).sort(),a=s.length>0?this._params[s[s.length-1]]:null;this._editValues=a?{...a}:{...St},this._editingYear=e,this._newYear=""}async _save(){if(null!=this._editingYear&&this.hass&&this.entryId){this._saving=!0;try{await this.hass.callWS({type:"my_solar_cells/set_yearly_params",entry_id:this.entryId,year:parseInt(this._editingYear,10),tax_reduction:this._editValues.tax_reduction??0,grid_compensation:this._editValues.grid_compensation??0,transfer_fee:this._editValues.transfer_fee??0,energy_tax:this._editValues.energy_tax??0,installed_kw:this._editValues.installed_kw??0}),this._editingYear=null,await this._fetchData()}catch(t){this._error=t.message||"Failed to save"}this._saving=!1}}async _delete(){if(null!=this._editingYear&&this.hass&&this.entryId&&confirm(`Delete parameters for ${this._editingYear}?`)){this._saving=!0;try{await this.hass.callWS({type:"my_solar_cells/delete_yearly_params",entry_id:this.entryId,year:parseInt(this._editingYear,10)}),this._editingYear=null,await this._fetchData()}catch(t){this._error=t.message||"Failed to delete"}this._saving=!1}}};At.styles=[vt,gt,o`
      .edit-form {
        display: grid;
        grid-template-columns: repeat(auto-fit, minmax(180px, 1fr));
        gap: 12px;
        margin-top: 12px;
      }

      .edit-form .input-group input {
        width: 100%;
        box-sizing: border-box;
      }

      .form-actions {
        display: flex;
        gap: 8px;
        margin-top: 16px;
        align-items: center;
      }

      .btn-danger {
        background: var(--error-color, #db4437) !important;
      }

      .btn-secondary {
        background: var(--secondary-text-color) !important;
      }

      .add-year-row {
        display: flex;
        gap: 8px;
        align-items: flex-end;
        margin-bottom: 16px;
      }

      .add-year-row .input-group select {
        width: 100px;
        padding: 6px 8px;
        border: 1px solid var(--divider-color, #e0e0e0);
        border-radius: 4px;
        background: var(--card-background-color, #fff);
        color: var(--primary-text-color);
        font-size: 14px;
      }

      tr.clickable {
        cursor: pointer;
      }

      tr.clickable:hover td {
        background: var(--primary-background-color);
      }

      tr.selected td {
        background: color-mix(in srgb, var(--primary-color) 12%, transparent);
      }
    `],t([pt({attribute:!1})],At.prototype,"hass",void 0),t([pt()],At.prototype,"entryId",void 0),t([ut()],At.prototype,"_params",void 0),t([ut()],At.prototype,"_loading",void 0),t([ut()],At.prototype,"_fetched",void 0),t([ut()],At.prototype,"_error",void 0),t([ut()],At.prototype,"_editingYear",void 0),t([ut()],At.prototype,"_editValues",void 0),t([ut()],At.prototype,"_newYear",void 0),t([ut()],At.prototype,"_saving",void 0),t([ut()],At.prototype,"_minYear",void 0),t([ut()],At.prototype,"_maxYear",void 0),At=t([lt("yearly-params-view")],At);let Et=class extends nt{constructor(){super(...arguments),this.entryId="",this._projection=[],this._investment=0,this._loading=!1,this._error="",this._initialLoaded=!1,this._defaultPriceDev=5,this._defaultPanelDeg=.25}updated(t){t.has("hass")&&this.hass&&this.entryId&&!this._initialLoaded&&!this._loading&&this._fetchInitial()}async _fetchInitial(){if(this.hass&&this.entryId){this._loading=!0,this._error="";try{const t=await this.hass.callWS({type:"my_solar_cells/get_roi_projection",entry_id:this.entryId});this._projection=t.projection,this._investment=t.investment,this._defaultPriceDev=t.price_development,this._defaultPanelDeg=t.panel_degradation,this._initialLoaded=!0}catch(t){this._error=t.message||"Failed to fetch ROI projection"}this._loading=!1}}async _onCalculate(){if(!this.hass||!this.entryId)return;const t=this.shadowRoot.getElementById("price-dev-input"),e=this.shadowRoot.getElementById("panel-deg-input"),s=parseFloat(t.value)||0,a=parseFloat(e.value)||0;this._loading=!0,this._error="";try{const t=await this.hass.callWS({type:"my_solar_cells/get_roi_projection",entry_id:this.entryId,price_development:s,panel_degradation:a});this._projection=t.projection,this._investment=t.investment}catch(t){this._error=t.message||"Failed to recalculate ROI projection"}this._loading=!1}_fmtInt(t){return Math.round(t).toLocaleString("sv-SE")}_fmtSek(t){return t.toLocaleString("sv-SE",{minimumFractionDigits:2,maximumFractionDigits:2})}_fmtPct(t){return t.toLocaleString("sv-SE",{minimumFractionDigits:1,maximumFractionDigits:1})}render(){return this._loading&&!this._initialLoaded?K`<div class="loading">Loading ROI projection...</div>`:this._error?K`<div class="no-data">Error: ${this._error}</div>`:this._projection.length?K`
      <div class="card">
        <h3>ROI Projection</h3>
        <div class="investment-info">
          Investment: <strong>${this._fmtSek(this._investment)} SEK</strong>
        </div>
        <details class="info-box">
          <summary>How is the ROI calculated?</summary>
          <ul>
            <li><strong>Historical years</strong> use actual production and price data from Tibber.
              If the current year is incomplete, missing months are filled with data from the
              previous year or average production estimates.</li>
            <li><strong>Sold price</strong> = spot price + grid compensation (n&auml;tnytta)
              + tax reduction (skattereduktion, until 2026).</li>
            <li><strong>Own use price</strong> = avoided purchase cost (spot price + transfer fee
              + energy tax). If a battery is present, it is the average of own-use and battery
              savings per kWh.</li>
            <li><strong>Future years</strong> are projected using monthly average prices and
              production from the last historical year. Price development and panel degradation
              are applied per month, then summed to yearly totals. This captures seasonal
              variation &mdash; summer has high production but typically lower prices, winter
              the opposite.</li>
            <ul>
              <li>Prices increase each year by the <em>price development</em> percentage.
                E.g. 5% means next year's price = this year's price &times; 1.05.</li>
              <li>Production decreases each year by the <em>panel degradation</em> percentage.
                E.g. 0.25% means next year's production = this year's &times; 0.9975.</li>
            </ul>
            <li><strong>Savings sold</strong> = production sold &times; average sold price</li>
            <li><strong>Savings own use</strong> = own use production &times; average own use price</li>
            <li><strong>Remaining</strong> = investment &minus; cumulative total savings</li>
            <li>The <strong>ROI year</strong> (green row) is when cumulative savings exceed the investment.</li>
            <li>Tax reduction (skattereduktion) is removed from sold price starting 2026.</li>
            <li><em>Note:</em> The table shows yearly weighted averages, but all savings
              calculations use monthly granularity for accuracy.</li>
          </ul>
        </details>
        <div class="table-controls">
          <div class="input-group">
            <label>Price development (%/year)</label>
            <input
              id="price-dev-input"
              type="number"
              step="0.5"
              value=${this._defaultPriceDev}
            />
          </div>
          <div class="input-group">
            <label>Panel degradation (%/year)</label>
            <input
              id="panel-deg-input"
              type="number"
              step="0.05"
              value=${this._defaultPanelDeg}
            />
          </div>
          <button class="btn" @click=${this._onCalculate} ?disabled=${this._loading}>
            ${this._loading?"Calculating...":"Calculate"}
          </button>
        </div>
        <div class="table-wrapper">
          <table>
            <thead>
              <tr>
                <th class="number">#</th>
                <th class="number">Year</th>
                <th class="number">Avg price sold</th>
                <th class="number">Avg price own use</th>
                <th class="number">Prod. sold (kWh)</th>
                <th class="number">Prod. own use (kWh)</th>
                <th class="number">Savings sold (SEK)</th>
                <th class="number">Savings own use (SEK)</th>
                <th class="number">Return %</th>
                <th class="number">Remaining (SEK)</th>
              </tr>
            </thead>
            <tbody>
              ${this._projection.map(t=>this._renderRow(t))}
            </tbody>
          </table>
        </div>
      </div>
    `:K`<div class="no-data">No ROI projection data available.</div>`}_renderRow(t){return K`
      <tr class=${t.is_roi_year?"roi-row":""}>
        <td class="number">${t.year_from_start}</td>
        <td class="number">${t.year}</td>
        <td class="number">${this._fmtSek(t.average_price_sold)}</td>
        <td class="number">${this._fmtSek(t.average_price_own_use)}</td>
        <td class="number">${this._fmtInt(t.production_sold)}</td>
        <td class="number">${this._fmtInt(t.production_own_use)}</td>
        <td class="number">${this._fmtSek(t.year_savings_sold)}</td>
        <td class="number">${this._fmtSek(t.year_savings_own_use)}</td>
        <td class="number">${this._fmtPct(t.return_percentage)}</td>
        <td class="number">${this._fmtSek(t.remaining_on_investment)}</td>
      </tr>
    `}};Et.styles=[vt,gt,o`
      .roi-row {
        background: rgba(76, 175, 80, 0.15);
        font-weight: 600;
      }

      td.number {
        text-align: right;
        font-variant-numeric: tabular-nums;
      }

      th.number {
        text-align: right;
      }

      .investment-info {
        font-size: 0.95em;
        color: var(--secondary-text-color);
        margin-bottom: 12px;
      }

      .investment-info strong {
        color: var(--primary-text-color);
      }

      .input-group input[type="number"] {
        width: 80px;
      }

      .info-box {
        background: var(--primary-background-color);
        border-left: 3px solid var(--primary-color);
        border-radius: 4px;
        padding: 12px 16px;
        margin-bottom: 16px;
        font-size: 0.85em;
        line-height: 1.5;
        color: var(--secondary-text-color);
      }

      .info-box summary {
        cursor: pointer;
        color: var(--primary-text-color);
        font-weight: 500;
      }

      .info-box ul {
        margin: 8px 0 0 0;
        padding-left: 20px;
      }

      .info-box li {
        margin-bottom: 4px;
      }
    `],t([pt({attribute:!1})],Et.prototype,"hass",void 0),t([pt()],Et.prototype,"entryId",void 0),t([ut()],Et.prototype,"_projection",void 0),t([ut()],Et.prototype,"_investment",void 0),t([ut()],Et.prototype,"_loading",void 0),t([ut()],Et.prototype,"_error",void 0),t([ut()],Et.prototype,"_initialLoaded",void 0),Et=t([lt("roi-view")],Et);const Dt={today:"Idag",day:"Dag",week:"Vecka",month:"Månad",year:"År"},Pt=["JANUARI","FEBRUARI","MARS","APRIL","MAJ","JUNI","JULI","AUGUSTI","SEPTEMBER","OKTOBER","NOVEMBER","DECEMBER"],It=["JAN.","FEB.","MAR.","APR.","MAJ","JUN.","JUL.","AUG.","SEP.","OKT.","NOV.","DEC."];let Tt=class extends nt{constructor(){super(...arguments),this.entryId="",this._period="week",this._currentDate=new Date,this._data=null,this._simData=null,this._loading=!1,this._simLoading=!1,this._error="",this._simEnabled=!1,this._simAddBattery=!0,this._simBatteryKwh=10,this._simRemoveTax=!1,this._initialLoaded=!1}updated(t){t.has("hass")&&this.hass&&this.entryId&&!this._initialLoaded&&!this._loading&&this._fetchData()}_getDateRange(){const t=new Date(this._currentDate);let e,s;switch(this._period){case"today":{const t=new Date;e=new Date(t.getFullYear(),t.getMonth(),t.getDate()),s=new Date(e),s.setDate(s.getDate()+1);break}case"day":e=new Date(t.getFullYear(),t.getMonth(),t.getDate()),s=new Date(e),s.setDate(s.getDate()+1);break;case"week":{const a=t.getDay(),i=0===a?6:a-1;e=new Date(t.getFullYear(),t.getMonth(),t.getDate()-i),s=new Date(e),s.setDate(s.getDate()+7);break}case"month":e=new Date(t.getFullYear(),t.getMonth(),1),s=new Date(t.getFullYear(),t.getMonth()+1,1);break;case"year":e=new Date(t.getFullYear(),0,1),s=new Date(t.getFullYear()+1,0,1)}return{start:this._toLocalIso(e),end:this._toLocalIso(s)}}_toLocalIso(t){return`${t.getFullYear()}-${String(t.getMonth()+1).padStart(2,"0")}-${String(t.getDate()).padStart(2,"0")}T00:00:00`}_getPeriodLabel(){const t=new Date(this._currentDate);switch(this._period){case"today":return"IDAG";case"day":return`${String(t.getDate()).padStart(2,"0")}/${It[t.getMonth()]} ${t.getFullYear()}`;case"week":{const e=t.getDay(),s=0===e?6:e-1,a=new Date(t.getFullYear(),t.getMonth(),t.getDate()-s),i=new Date(a);i.setDate(i.getDate()+6);return`${`${String(a.getDate()).padStart(2,"0")}/${It[a.getMonth()]}`}-${`${String(i.getDate()).padStart(2,"0")}/${It[i.getMonth()]}`}`}case"month":return`${Pt[t.getMonth()]} ${t.getFullYear()}`;case"year":return`${t.getFullYear()}`}}_navigate(t){const e=new Date(this._currentDate);switch(this._period){case"day":e.setDate(e.getDate()+t);break;case"week":e.setDate(e.getDate()+7*t);break;case"month":e.setMonth(e.getMonth()+t);break;case"year":e.setFullYear(e.getFullYear()+t)}this._currentDate=e,this._fetchData()}_setPeriod(t){this._period=t,this._currentDate=new Date,this._simData=null,this._fetchData()}async _fetchData(){if(this.hass&&this.entryId){this._loading=!0,this._error="";try{const t=this._getDateRange(),e=await this.hass.callWS({type:"my_solar_cells/get_fakta_breakdown",entry_id:this.entryId,start_date:t.start,end_date:t.end});this._data=e,this._initialLoaded=!0}catch(t){this._error=t.message||"Failed to fetch data"}this._loading=!1}}async _simulate(){if(this.hass&&this.entryId){this._simLoading=!0;try{const t=this._getDateRange(),e=await this.hass.callWS({type:"my_solar_cells/simulate_fakta",entry_id:this.entryId,start_date:t.start,end_date:t.end,add_battery:this._simAddBattery,battery_kwh:this._simBatteryKwh,remove_tax_reduction:this._simRemoveTax});this._simData=e,this._simEnabled=!0}catch(t){this._error=t.message||"Simulation failed"}this._simLoading=!1}}_fmtKwh(t){return t.toLocaleString("sv-SE",{minimumFractionDigits:2,maximumFractionDigits:2})+" kWh"}_fmtSek(t){return t.toLocaleString("sv-SE",{minimumFractionDigits:2,maximumFractionDigits:2})+" Sek"}_fmtSekPerKwh(t){return t.toLocaleString("sv-SE",{minimumFractionDigits:2,maximumFractionDigits:2})+" Sek"}render(){if(this._loading&&!this._initialLoaded)return K`<div class="loading">Loading...</div>`;if(this._error&&!this._data)return K`<div class="no-data">Error: ${this._error}</div>`;const t=this._simEnabled&&this._simData?this._simData:this._data;return K`
      ${this._renderPeriodNav()}
      ${t?this._renderColumns(t):K`<div class="no-data">No data for this period</div>`}
    `}_renderPeriodNav(){const t="today"!==this._period;return K`
      <div class="period-nav">
        <div class="period-tabs">
          ${Object.keys(Dt).map(t=>K`
              <button
                class="period-tab ${this._period===t?"active":""}"
                @click=${()=>this._setPeriod(t)}
              >
                ${Dt[t]}
              </button>
            `)}
        </div>
        ${t?K`
              <button class="nav-arrow" @click=${()=>this._navigate(-1)}>&larr;</button>
              <span class="period-label">${this._getPeriodLabel()}</span>
              <button class="nav-arrow" @click=${()=>this._navigate(1)}>&rarr;</button>
            `:H}
      </div>
    `}_renderColumns(t){return K`
      <div class="fakta-columns">
        ${this._renderProductionColumn(t)}
        ${this._renderCostColumn(t)}
        ${this._renderSimAndFactsColumn(t)}
      </div>
    `}_renderProductionColumn(t){return K`
      <div class="fakta-card">
        <h3>Produktion och konsumtion</h3>
        <div class="fakta-row"><span class="label">S\u00e5ld</span><span class="value">${this._fmtKwh(t.production_sold)}</span></div>
        <div class="fakta-row"><span class="label">Eget anv.</span><span class="value">${this._fmtKwh(t.production_own_use)}</span></div>
        <div class="fakta-row"><span class="label">Batteriladdning</span><span class="value">${this._fmtKwh(t.battery_charge)}</span></div>
        <div class="fakta-row"><span class="label">Batteri anv.</span><span class="value">${this._fmtKwh(t.battery_used)}</span></div>
        <div class="fakta-row"><span class="label">K\u00f6pt</span><span class="value">${this._fmtKwh(t.purchased)}</span></div>
        <hr class="fakta-separator" />
        <div class="fakta-row fakta-summary"><span class="label">Produktion</span><span class="value">${this._fmtKwh(t.sum_all_production)}</span></div>
        <div class="fakta-row fakta-summary"><span class="label">Konsumtion</span><span class="value">${this._fmtKwh(t.sum_all_consumption)}</span></div>
        <div class="fakta-row fakta-summary"><span class="label">Balans (prod. - f\u00f6rb.)</span><span class="value">${this._fmtKwh(t.sum_all_production-t.sum_all_consumption)}</span></div>
      </div>
    `}_renderCostColumn(t){return K`
      <div class="fakta-card">
        <h3>Kostnader och int\u00e4kter</h3>
        <div class="fakta-row"><span class="label">Prod s\u00e5lt</span><span class="value">${this._fmtSek(t.production_sold_profit)}</span></div>
        <div class="fakta-row"><span class="label">Prod n\u00e4tnytta</span><span class="value">${this._fmtSek(t.production_sold_grid_compensation_profit)}</span></div>
        <div class="fakta-row"><span class="label">Prod energiskatt</span><span class="value">${this._fmtSek(t.production_sold_tax_reduction_profit)}</span></div>
        <div class="fakta-row"><span class="label">Eget anv. spotpris</span><span class="value">${this._fmtSek(t.production_own_use_saved)}</span></div>
        <div class="fakta-row"><span class="label">Eget anv. \u00f6verf\u00f6ring</span><span class="value">${this._fmtSek(t.production_own_use_transfer_fee_saved)}</span></div>
        <div class="fakta-row"><span class="label">Eget anv. energiskatt</span><span class="value">${this._fmtSek(t.production_own_use_energy_tax_saved)}</span></div>
        <div class="fakta-row"><span class="label">Batt. anv. spotpris</span><span class="value">${this._fmtSek(t.battery_used_saved)}</span></div>
        <div class="fakta-row"><span class="label">Batt. anv. \u00f6verf\u00f6ring</span><span class="value">${this._fmtSek(t.battery_use_transfer_fee_saved)}</span></div>
        <div class="fakta-row"><span class="label">Batt. anv. energiskatt</span><span class="value">${this._fmtSek(t.battery_use_energy_tax_saved)}</span></div>
        <hr class="fakta-separator" />
        <div class="fakta-row"><span class="label">K\u00f6pt kostnad</span><span class="value">${this._fmtSek(-t.purchased_cost)}</span></div>
        <div class="fakta-row"><span class="label">K\u00f6pt \u00f6verf\u00f6ring</span><span class="value">${this._fmtSek(-t.purchased_transfer_fee_cost)}</span></div>
        <div class="fakta-row"><span class="label">K\u00f6pt energiskatt</span><span class="value">${this._fmtSek(-t.purchased_tax_cost)}</span></div>
        <hr class="fakta-separator" />
        <div class="fakta-row fakta-summary"><span class="label">Inkl. kostnad</span><span class="value">${this._fmtSek(t.sum_all_production_sold_and_saved)}</span></div>
        <div class="fakta-row fakta-summary"><span class="label">Exkl. kostnad</span><span class="value">${this._fmtSek(t.sum_all_production_sold_and_saved-t.sum_purchased_cost)}</span></div>
        <hr class="fakta-separator" />
        <div class="fakta-row fakta-summary"><span class="label">Produktion</span><span class="value">${this._fmtSek(t.sum_all_production_sold_and_saved)}</span></div>
        <div class="fakta-row fakta-summary"><span class="label">Konsumtion</span><span class="value">${this._fmtSek(-t.sum_purchased_cost)}</span></div>
        <div class="fakta-row fakta-summary"><span class="label">Batt. (prod. - f\u00f6rb.)</span><span class="value">${this._fmtSek(t.balance)}</span></div>
      </div>
    `}_renderSimAndFactsColumn(t){return K`
      <div class="fakta-card">
        <h3>Simulering</h3>
        <div class="sim-section">
          <div class="sim-toggle-group">
            <button
              class="sim-toggle ${this._simAddBattery?"active":""}"
              @click=${()=>{this._simAddBattery=!0}}
            >L\u00e4gg till batteri</button>
            <button
              class="sim-toggle ${this._simAddBattery?"":"active"}"
              @click=${()=>{this._simAddBattery=!1}}
            >Ta bort batteri</button>
          </div>
          ${this._simAddBattery?K`
                <div class="sim-slider-row">
                  <input
                    type="range"
                    min="1"
                    max="30"
                    step="1"
                    .value=${String(this._simBatteryKwh)}
                    @input=${t=>{this._simBatteryKwh=parseInt(t.target.value)}}
                  />
                  <span class="slider-value">${this._simBatteryKwh} kWh</span>
                </div>
              `:H}
          <label class="sim-checkbox">
            <input
              type="checkbox"
              .checked=${this._simRemoveTax}
              @change=${t=>{this._simRemoveTax=t.target.checked}}
            />
            Ta bort skattereduktionen
          </label>
          <button
            class="sim-btn"
            @click=${this._simulate}
            ?disabled=${this._simLoading}
          >
            ${this._simLoading?"Beräknar...":"Beräkna"}
          </button>
        </div>

        <h3>Fakta</h3>
        <div class="fakta-row"><span class="label">Produktionsindex (prod/dag)</span><span class="value">${this._fmtKwh(t.facts_production_index)}</span></div>
        <div class="fakta-row"><span class="label">Snittpris s\u00e5ld</span><span class="value">${this._fmtSekPerKwh(t.facts_production_sold_avg_per_kwh_profit)}</span></div>
        <div class="fakta-row"><span class="label">Snittpris k\u00f6pt</span><span class="value">${this._fmtSekPerKwh(t.facts_purchased_cost_avg_per_kwh)}</span></div>
        <div class="fakta-row"><span class="label">Snittpris eget anv\u00e4ndning</span><span class="value">${this._fmtSekPerKwh(t.facts_production_own_use_avg_per_kwh_saved)}</span></div>
        <div class="fakta-row"><span class="label">Eget anv. reducering avg. f\u00f6rb.</span><span class="value">${this._fmtKwh(t.peak_energy_reduction)}</span></div>
      </div>
    `}};Tt.styles=[mt],t([pt({attribute:!1})],Tt.prototype,"hass",void 0),t([pt()],Tt.prototype,"entryId",void 0),t([ut()],Tt.prototype,"_period",void 0),t([ut()],Tt.prototype,"_currentDate",void 0),t([ut()],Tt.prototype,"_data",void 0),t([ut()],Tt.prototype,"_simData",void 0),t([ut()],Tt.prototype,"_loading",void 0),t([ut()],Tt.prototype,"_simLoading",void 0),t([ut()],Tt.prototype,"_error",void 0),t([ut()],Tt.prototype,"_simEnabled",void 0),t([ut()],Tt.prototype,"_simAddBattery",void 0),t([ut()],Tt.prototype,"_simBatteryKwh",void 0),t([ut()],Tt.prototype,"_simRemoveTax",void 0),t([ut()],Tt.prototype,"_initialLoaded",void 0),Tt=t([lt("fakta-view")],Tt);let Ct=class extends nt{constructor(){super(...arguments),this._activeTab="overview"}get _entryId(){return this.panel?.config?.entry_id||""}render(){return K`
      <div class="content">
        <div class="header">
          <h1>Solar Data</h1>
        </div>
        <div class="tabs">
          <button
            class="tab ${"overview"===this._activeTab?"active":""}"
            @click=${()=>this._activeTab="overview"}
          >
            Overview
          </button>
          <button
            class="tab ${"roi"===this._activeTab?"active":""}"
            @click=${()=>this._activeTab="roi"}
          >
            ROI
          </button>
          <button
            class="tab ${"fakta"===this._activeTab?"active":""}"
            @click=${()=>this._activeTab="fakta"}
          >
            Fakta
          </button>
          <button
            class="tab ${"sensors"===this._activeTab?"active":""}"
            @click=${()=>this._activeTab="sensors"}
          >
            Sensors
          </button>
          <button
            class="tab ${"params"===this._activeTab?"active":""}"
            @click=${()=>this._activeTab="params"}
          >
            Yearly Params
          </button>
          <button
            class="tab ${"hourly"===this._activeTab?"active":""}"
            @click=${()=>this._activeTab="hourly"}
          >
            Hourly Energy
          </button>
        </div>
        <div class="tab-content" ?active=${"overview"===this._activeTab}>
          <overview-view
            .hass=${this.hass}
            .entryId=${this._entryId}
          ></overview-view>
        </div>
        <div class="tab-content" ?active=${"roi"===this._activeTab}>
          <roi-view
            .hass=${this.hass}
            .entryId=${this._entryId}
          ></roi-view>
        </div>
        <div class="tab-content" ?active=${"fakta"===this._activeTab}>
          <fakta-view
            .hass=${this.hass}
            .entryId=${this._entryId}
          ></fakta-view>
        </div>
        <div class="tab-content" ?active=${"sensors"===this._activeTab}>
          <sensors-view
            .hass=${this.hass}
            .entryId=${this._entryId}
          ></sensors-view>
        </div>
        <div class="tab-content" ?active=${"params"===this._activeTab}>
          <yearly-params-view
            .hass=${this.hass}
            .entryId=${this._entryId}
          ></yearly-params-view>
        </div>
        <div class="tab-content" ?active=${"hourly"===this._activeTab}>
          <hourly-energy-view
            .hass=${this.hass}
            .entryId=${this._entryId}
          ></hourly-energy-view>
        </div>
      </div>
    `}};Ct.styles=[_t,o`
      .content {
        max-width: 1200px;
        margin: 0 auto;
      }

      .tab-content {
        display: none;
      }

      .tab-content[active] {
        display: block;
      }
    `],t([pt({attribute:!1})],Ct.prototype,"hass",void 0),t([pt({attribute:!1})],Ct.prototype,"narrow",void 0),t([pt({attribute:!1})],Ct.prototype,"route",void 0),t([pt({attribute:!1})],Ct.prototype,"panel",void 0),t([ut()],Ct.prototype,"_activeTab",void 0),Ct=t([lt("my-solar-cells-panel")],Ct);export{Ct as MySolarCellsPanel};
